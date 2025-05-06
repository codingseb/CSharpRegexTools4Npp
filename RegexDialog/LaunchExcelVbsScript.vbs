' Check arguments
If WScript.Arguments.Count < 3 Then
    WScript.Echo "Usage: cscript open_excel.vbs ""file_path"" ""sheet_name"" ""cell"""
    WScript.Echo "Example: cscript open_excel.vbs ""C:\MyFile.xlsx"" ""My Sheet"" ""B5"""
    WScript.Quit
End If

' Get parameters
filePath = WScript.Arguments(0)
sheetName = WScript.Arguments(1)
cell = WScript.Arguments(2)

' Check if file exists
Set fso = CreateObject("Scripting.FileSystemObject")
If Not fso.FileExists(filePath) Then
    WScript.Echo "Error: File '" & filePath & "' does not exist"
    WScript.Quit
End If

On Error Resume Next

' Try to connect to already opened file
Set xl = GetObject(,"Excel.Application")

If Err.Number <> 0 Then
    Set xl = CreateObject("Excel.Application")
    xl.Visible = True
    Set wb = xl.Workbooks.Open(filePath)
Else
    fileFound = False
    For Each wb In xl.Workbooks
        If UCase(wb.FullName) = UCase(filePath) Then
            fileFound = True
            Set wb = wb
            Exit For
        End If
    Next
    
    ' Si le fichier n'est pas ouvert, l'ouvrir
    If Not fileFound Then
        Set wb = xl.Workbooks.Open(filePath)
    End If
End If

' Activate sheet and cell
xl.Visible = True

' Force Excel to the foreground
Set sh = CreateObject("WScript.Shell")
sh.AppActivate xl.Caption

On Error Resume Next
Set ws = xl.Sheets(sheetName)
ws.Activate
Set rng = ws.Range(cell)
rng.Activate
rng.Select
