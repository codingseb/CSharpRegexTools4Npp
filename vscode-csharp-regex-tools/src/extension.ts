import * as vscode from 'vscode';
import * as path from 'path';
import { spawn, ChildProcess } from 'child_process';
import {
    createMessageConnection,
    StreamMessageReader,
    StreamMessageWriter,
    MessageConnection,
    RequestType0,
    RequestType,
} from 'vscode-jsonrpc/node';

// Request types for editor/* methods (C# host -> VS Code)
const editorGetText = new RequestType0<string, void>('editor/getText');
const editorSetText = new RequestType<{ text: string }, void, void>('editor/setText');
const editorSetTextInNew = new RequestType<{ text: string }, void, void>('editor/setTextInNew');
const editorGetSelectedText = new RequestType0<string, void>('editor/getSelectedText');
const editorSetSelectedText = new RequestType<{ text: string }, void, void>('editor/setSelectedText');
const editorSetPosition = new RequestType<{ index: number; length: number }, void, void>('editor/setPosition');
const editorSetSelection = new RequestType<{ index: number; length: number }, void, void>('editor/setSelection');
const editorGetSelectionStartIndex = new RequestType0<number, void>('editor/getSelectionStartIndex');
const editorGetSelectionLength = new RequestType0<number, void>('editor/getSelectionLength');
const editorSaveCurrentDocument = new RequestType0<void, void>('editor/saveCurrentDocument');
const editorSetCSharpHighlighting = new RequestType0<void, void>('editor/setCSharpHighlighting');
const editorTryOpen = new RequestType<{ fileName: string; onlyIfAlreadyOpen: boolean }, boolean, void>('editor/tryOpen');
const editorGetCurrentFileName = new RequestType0<string, void>('editor/getCurrentFileName');

let childProcess: ChildProcess | null = null;
let connection: MessageConnection | null = null;

function getExecutablePath(context: vscode.ExtensionContext): string {
    const configPath = vscode.workspace.getConfiguration('csharpRegexTools').get<string>('executablePath');
    if (configPath) {
        return configPath;
    }

    const arch = process.arch === 'ia32' ? 'x86' : 'x64';
    return path.join(context.extensionPath, 'bin', arch, 'CSharpRegexTools4VsCode.exe');
}

function offsetToPosition(document: vscode.TextDocument, offset: number): vscode.Position {
    return document.positionAt(offset);
}

function getFullRange(document: vscode.TextDocument): vscode.Range {
    return new vscode.Range(
        document.positionAt(0),
        document.positionAt(document.getText().length)
    );
}

function getActiveEditor(): vscode.TextEditor {
    const editor = vscode.window.activeTextEditor;
    if (!editor) {
        throw new Error('No active text editor');
    }
    return editor;
}

function registerEditorHandlers(conn: MessageConnection) {
    conn.onRequest(editorGetText, () => {
        return getActiveEditor().document.getText();
    });

    conn.onRequest(editorSetText, async ({ text }) => {
        const editor = getActiveEditor();
        await editor.edit(editBuilder => {
            editBuilder.replace(getFullRange(editor.document), text);
        });
    });

    conn.onRequest(editorSetTextInNew, async ({ text }) => {
        const doc = await vscode.workspace.openTextDocument({ content: text });
        await vscode.window.showTextDocument(doc);
    });

    conn.onRequest(editorGetSelectedText, () => {
        const editor = getActiveEditor();
        return editor.document.getText(editor.selection);
    });

    conn.onRequest(editorSetSelectedText, async ({ text }) => {
        const editor = getActiveEditor();
        await editor.edit(editBuilder => {
            editBuilder.replace(editor.selection, text);
        });
    });

    conn.onRequest(editorSetPosition, async ({ index, length }) => {
        const editor = getActiveEditor();
        const start = offsetToPosition(editor.document, index);
        const end = offsetToPosition(editor.document, index + length);
        editor.selection = new vscode.Selection(start, end);
        editor.revealRange(new vscode.Range(start, end), vscode.TextEditorRevealType.InCenterIfOutsideViewport);
        await vscode.window.showTextDocument(editor.document, editor.viewColumn);
    });

    conn.onRequest(editorSetSelection, ({ index, length }) => {
        const editor = getActiveEditor();
        const start = offsetToPosition(editor.document, index);
        const end = offsetToPosition(editor.document, index + length);
        const newSelection = new vscode.Selection(start, end);
        editor.selections = [...editor.selections, newSelection];
    });

    conn.onRequest(editorGetSelectionStartIndex, () => {
        const editor = getActiveEditor();
        return editor.document.offsetAt(editor.selection.start);
    });

    conn.onRequest(editorGetSelectionLength, () => {
        const editor = getActiveEditor();
        return editor.document.offsetAt(editor.selection.end) - editor.document.offsetAt(editor.selection.start);
    });

    conn.onRequest(editorSaveCurrentDocument, async () => {
        await getActiveEditor().document.save();
    });

    conn.onRequest(editorSetCSharpHighlighting, async () => {
        const editor = getActiveEditor();
        await vscode.languages.setTextDocumentLanguage(editor.document, 'csharp');
    });

    conn.onRequest(editorTryOpen, async ({ fileName, onlyIfAlreadyOpen }) => {
        // Check if already open
        for (const doc of vscode.workspace.textDocuments) {
            if (doc.fileName.toLowerCase() === fileName.toLowerCase()) {
                await vscode.window.showTextDocument(doc);
                return true;
            }
        }

        if (onlyIfAlreadyOpen) {
            return false;
        }

        try {
            const doc = await vscode.workspace.openTextDocument(fileName);
            await vscode.window.showTextDocument(doc);
            return true;
        } catch {
            return false;
        }
    });

    conn.onRequest(editorGetCurrentFileName, () => {
        return getActiveEditor().document.fileName;
    });
}

function startHost(context: vscode.ExtensionContext) {
    const exePath = getExecutablePath(context);

    childProcess = spawn(exePath, [], {
        stdio: ['pipe', 'pipe', 'pipe'],
    });

    childProcess.stderr?.on('data', (data: Buffer) => {
        console.error(`[CSharpRegexTools] ${data.toString()}`);
    });

    childProcess.on('exit', (code) => {
        console.log(`[CSharpRegexTools] Process exited with code ${code}`);
        connection = null;
        childProcess = null;
    });

    connection = createMessageConnection(
        new StreamMessageReader(childProcess.stdout!),
        new StreamMessageWriter(childProcess.stdin!)
    );

    registerEditorHandlers(connection);
    connection.listen();

    connection.sendNotification('window/show');
}

export function activate(context: vscode.ExtensionContext) {
    const disposable = vscode.commands.registerCommand('csharpRegexTools.open', () => {
        if (connection && childProcess && !childProcess.killed) {
            connection.sendNotification('window/show');
        } else {
            startHost(context);
        }
    });

    context.subscriptions.push(disposable);
}

export function deactivate() {
    if (connection) {
        try {
            connection.sendNotification('shutdown');
        } catch {
            // Process may already be dead
        }
        connection.dispose();
        connection = null;
    }

    if (childProcess && !childProcess.killed) {
        childProcess.kill();
        childProcess = null;
    }
}
