﻿// NPP plugin platform for .Net v0.94.00 by Kasper B. Graversen etc.
//
// This file should stay in sync with the CPP project file
// "notepad-plus-plus/scintilla/include/Scintilla.iface"
// found at
// https://github.com/notepad-plus-plus/notepad-plus-plus/blob/master/scintilla/include/Scintilla.iface
using CSharpRegexTools4Npp.PluginInfrastructure;

namespace CSharpRegexTools4Npp.PluginInfrastructure
{
	public enum Resource
	{
		/* ++Autogenerated -- start of section automatically generated from resource.h */

        IDC_STATIC = -1,

        IDI_M30ICON = 100,
        IDI_CHAMELEON = 101,

        IDR_RT_MANIFEST = 103,

        IDI_NEW_OFF_ICON = 201,
        IDI_OPEN_OFF_ICON = 202,
        IDI_CLOSE_OFF_ICON = 203,
        IDI_CLOSEALL_OFF_ICON = 204,
        IDI_SAVE_OFF_ICON = 205,
        IDI_SAVEALL_OFF_ICON = 206,
        IDI_CUT_OFF_ICON = 207,
        IDI_COPY_OFF_ICON = 208,
        IDI_PASTE_OFF_ICON = 209,
        IDI_UNDO_OFF_ICON = 210,
        IDI_REDO_OFF_ICON = 211,
        IDI_FIND_OFF_ICON = 212,
        IDI_REPLACE_OFF_ICON = 213,
        IDI_ZOOMIN_OFF_ICON = 214,
        IDI_ZOOMOUT_OFF_ICON = 215,
        IDI_VIEW_UD_DLG_OFF_ICON = 216,
        IDI_PRINT_OFF_ICON = 217,
        IDI_VIEW_ALL_CHAR_ON_ICON = 218,
        IDI_VIEW_INDENT_ON_ICON = 219,
        IDI_VIEW_WRAP_ON_ICON = 220,

        IDI_STARTRECORD_OFF_ICON = 221,
        IDI_STARTRECORD_ON_ICON = 222,
        IDI_STARTRECORD_DISABLE_ICON = 223,
        IDI_STOPRECORD_OFF_ICON = 224,
        IDI_STOPRECORD_ON_ICON = 225,
        IDI_STOPRECORD_DISABLE_ICON = 226,
        IDI_PLAYRECORD_OFF_ICON = 227,
        IDI_PLAYRECORD_ON_ICON = 228,
        IDI_PLAYRECORD_DISABLE_ICON = 229,
        IDI_SAVERECORD_OFF_ICON = 230,
        IDI_SAVERECORD_ON_ICON = 231,
        IDI_SAVERECORD_DISABLE_ICON = 232,

        IDI_MMPLAY_DIS_ICON = 233,
        IDI_MMPLAY_OFF_ICON = 234,
        IDI_MMPLAY_ON_ICON = 235,

        IDI_NEW_ON_ICON = 301,
        IDI_OPEN_ON_ICON = 302,
        IDI_CLOSE_ON_ICON = 303,
        IDI_CLOSEALL_ON_ICON = 304,
        IDI_SAVE_ON_ICON = 305,
        IDI_SAVEALL_ON_ICON = 306,
        IDI_CUT_ON_ICON = 307,
        IDI_COPY_ON_ICON = 308,
        IDI_PASTE_ON_ICON = 309,
        IDI_UNDO_ON_ICON = 310,
        IDI_REDO_ON_ICON = 311,
        IDI_FIND_ON_ICON = 312,
        IDI_REPLACE_ON_ICON = 313,
        IDI_ZOOMIN_ON_ICON = 314,
        IDI_ZOOMOUT_ON_ICON = 315,
        IDI_VIEW_UD_DLG_ON_ICON = 316,
        IDI_PRINT_ON_ICON = 317,
        IDI_VIEW_ALL_CHAR_OFF_ICON = 318,
        IDI_VIEW_INDENT_OFF_ICON = 319,
        IDI_VIEW_WRAP_OFF_ICON = 320,

        IDI_SAVE_DISABLE_ICON = 403,
        IDI_SAVEALL_DISABLE_ICON = 404,

        IDI_CUT_DISABLE_ICON = 407,
        IDI_COPY_DISABLE_ICON = 408,
        IDI_PASTE_DISABLE_ICON = 409,
        IDI_UNDO_DISABLE_ICON = 410,
        IDI_REDO_DISABLE_ICON = 411,
        IDI_DELETE_ICON = 412,

        IDI_SYNCV_OFF_ICON = 413,
        IDI_SYNCV_ON_ICON = 414,
        IDI_SYNCV_DISABLE_ICON = 415,

        IDI_SYNCH_OFF_ICON = 416,
        IDI_SYNCH_ON_ICON = 417,
        IDI_SYNCH_DISABLE_ICON = 418,

        IDI_SAVED_ICON = 501,
        IDI_UNSAVED_ICON = 502,
        IDI_READONLY_ICON = 503,
        IDI_FIND_RESULT_ICON = 504,
        IDI_MONITORING_ICON = 505,

        IDI_PROJECT_WORKSPACE = 601,
        IDI_PROJECT_WORKSPACEDIRTY = 602,
        IDI_PROJECT_PROJECT = 603,
        IDI_PROJECT_FOLDEROPEN = 604,
        IDI_PROJECT_FOLDERCLOSE = 605,
        IDI_PROJECT_FILE = 606,
        IDI_PROJECT_FILEINVALID = 607,
        IDI_FB_ROOTOPEN = 608,
        IDI_FB_ROOTCLOSE = 609,
        IDI_FB_SELECTCURRENTFILE = 610,
        IDI_FB_FOLDALL = 611,
        IDI_FB_EXPANDALL = 612,

        IDI_FUNCLIST_ROOT = 620,
        IDI_FUNCLIST_NODE = 621,
        IDI_FUNCLIST_LEAF = 622,

        IDI_FUNCLIST_SORTBUTTON = 631,
        IDI_FUNCLIST_RELOADBUTTON = 632,

        IDI_VIEW_DOC_MAP_ON_ICON = 633,
        IDI_VIEW_DOC_MAP_OFF_ICON = 634,
        IDI_VIEW_FILEBROWSER_ON_ICON = 635,
        IDI_VIEW_FILEBROWSER_OFF_ICON = 636,
        IDI_VIEW_FUNCLIST_ON_ICON = 637,
        IDI_VIEW_FUNCLIST_OFF_ICON = 638,
        IDI_VIEW_MONITORING_ON_ICON = 639,
        IDI_VIEW_MONITORING_OFF_ICON = 640,

        IDC_MY_CUR = 1402,
        IDC_UP_ARROW = 1403,
        IDC_DRAG_TAB = 1404,
        IDC_DRAG_INTERDIT_TAB = 1405,
        IDC_DRAG_PLUS_TAB = 1406,
        IDC_DRAG_OUT_TAB = 1407,

        IDC_MACRO_RECORDING = 1408,

        IDR_SAVEALL = 1500,
        IDR_CLOSEFILE = 1501,
        IDR_CLOSEALL = 1502,
        IDR_FIND = 1503,
        IDR_REPLACE = 1504,
        IDR_ZOOMIN = 1505,
        IDR_ZOOMOUT = 1506,
        IDR_WRAP = 1507,
        IDR_INVISIBLECHAR = 1508,
        IDR_INDENTGUIDE = 1509,
        IDR_SHOWPANNEL = 1510,
        IDR_STARTRECORD = 1511,
        IDR_STOPRECORD = 1512,
        IDR_PLAYRECORD = 1513,
        IDR_SAVERECORD = 1514,
        IDR_SYNCV = 1515,
        IDR_SYNCH = 1516,
        IDR_FILENEW = 1517,
        IDR_FILEOPEN = 1518,
        IDR_FILESAVE = 1519,
        IDR_PRINT = 1520,
        IDR_CUT = 1521,
        IDR_COPY = 1522,
        IDR_PASTE = 1523,
        IDR_UNDO = 1524,
        IDR_REDO = 1525,
        IDR_M_PLAYRECORD = 1526,
        IDR_DOCMAP = 1527,
        IDR_FUNC_LIST = 1528,
        IDR_FILEBROWSER = 1529,
        IDR_CLOSETAB = 1530,
        IDR_CLOSETAB_INACT = 1531,
        IDR_CLOSETAB_HOVER = 1532,
        IDR_CLOSETAB_PUSH = 1533,
        IDR_FUNC_LIST_ICO = 1534,
        IDR_DOCMAP_ICO = 1535,
        IDR_PROJECTPANEL_ICO = 1536,
        IDR_CLIPBOARDPANEL_ICO = 1537,
        IDR_ASCIIPANEL_ICO = 1538,
        IDR_DOCSWITCHER_ICO = 1539,
        IDR_FILEBROWSER_ICO = 1540,
        IDR_FILEMONITORING = 1541,

        ID_MACRO = 20000,
        ID_MACRO_LIMIT = 20200,

        ID_USER_CMD = 21000,
        ID_USER_CMD_LIMIT = 21200,

        ID_PLUGINS_CMD = 22000,
        ID_PLUGINS_CMD_LIMIT = 22500,

        ID_PLUGINS_CMD_DYNAMIC = 23000,
        ID_PLUGINS_CMD_DYNAMIC_LIMIT = 24999,

        MARKER_PLUGINS = 3,
        MARKER_PLUGINS_LIMIT = 19,

        ID_PLUGINS_REMOVING = 22501,
        ID_PLUGINS_REMOVING_END = 22600,

        IDCMD = 50000,

        IDC_PREV_DOC = IDCMD+3,
        IDC_NEXT_DOC = IDCMD+4,
        IDC_EDIT_TOGGLEMACRORECORDING = IDCMD+5,

        IDCMD_LIMIT = IDCMD+20,

        IDSCINTILLA = 60000,
        IDSCINTILLA_KEY_HOME = IDSCINTILLA+0,
        IDSCINTILLA_KEY_HOME_WRAP = IDSCINTILLA+1,
        IDSCINTILLA_KEY_END = IDSCINTILLA+2,
        IDSCINTILLA_KEY_END_WRAP = IDSCINTILLA+3,
        IDSCINTILLA_KEY_LINE_DUP = IDSCINTILLA+4,
        IDSCINTILLA_KEY_LINE_CUT = IDSCINTILLA+5,
        IDSCINTILLA_KEY_LINE_DEL = IDSCINTILLA+6,
        IDSCINTILLA_KEY_LINE_TRANS = IDSCINTILLA+7,
        IDSCINTILLA_KEY_LINE_COPY = IDSCINTILLA+8,
        IDSCINTILLA_KEY_CUT = IDSCINTILLA+9,
        IDSCINTILLA_KEY_COPY = IDSCINTILLA+10,
        IDSCINTILLA_KEY_PASTE = IDSCINTILLA+11,
        IDSCINTILLA_KEY_DEL = IDSCINTILLA+12,
        IDSCINTILLA_KEY_SELECTALL = IDSCINTILLA+13,
        IDSCINTILLA_KEY_OUTDENT = IDSCINTILLA+14,
        IDSCINTILLA_KEY_UNDO = IDSCINTILLA+15,
        IDSCINTILLA_KEY_REDO = IDSCINTILLA+16,
        IDSCINTILLA_LIMIT = IDSCINTILLA+30,

        IDD_FILEVIEW_DIALOG = 1000,

        IDC_MINIMIZED_TRAY = 67001,

        IDD_CREATE_DIRECTORY = 1100,
        IDC_STATIC_CURRENT_FOLDER = 1101,
        IDC_EDIT_NEW_FOLDER = 1102,

        IDD_INSERT_INPUT_TEXT = 1200,
        IDC_EDIT_INPUT_VALUE = 1201,
        IDC_STATIC_INPUT_TITLE = 1202,
        IDC_ICON_INPUT_ICON = 1203,

        IDR_M30_MENU = 1500,

        IDR_SYSTRAYPOPUP_MENU = 1501,

        IDD_ABOUTBOX = 1700,
        IDC_LICENCE_EDIT = 1701,
        IDC_HOME_ADDR = 1702,
        IDC_EMAIL_ADDR = 1703,
        IDC_ONLINEHELP_ADDR = 1704,
        IDC_AUTHOR_NAME = 1705,
        IDC_BUILD_DATETIME = 1706,
        IDC_VERSION_BIT = 1707,

        IDD_DEBUGINFOBOX = 1750,
        IDC_DEBUGINFO_EDIT = 1751,
        IDC_DEBUGINFO_COPYLINK = 1752,

        IDD_DOSAVEORNOTBOX = 1760,
        IDC_DOSAVEORNOTTEX = 1761,

        IDD_GOLINE = 2000,
        ID_GOLINE_EDIT = IDD_GOLINE + 1,
        ID_CURRLINE = IDD_GOLINE + 2,
        ID_LASTLINE = IDD_GOLINE + 3,
        ID_URHERE_STATIC = IDD_GOLINE + 4,
        ID_UGO_STATIC = IDD_GOLINE + 5,
        ID_NOMORETHAN_STATIC = IDD_GOLINE + 6,
        IDC_RADIO_GOTOLINE = IDD_GOLINE + 7,
        IDC_RADIO_GOTOOFFSET = IDD_GOLINE + 8,

        IDD_VALUE_DLG = 2400,
        IDC_VALUE_STATIC = 2401,
        IDC_VALUE_EDIT = 2402,

        IDD_BUTTON_DLG = 2410,
        IDC_RESTORE_BUTTON = 2411,

        IDD_SETTING_DLG = 2500,

        NOTEPADPLUS_USER_INTERNAL = Constants.WM_USER + 0000,
        NPPM_INTERNAL_USERCMDLIST_MODIFIED = NOTEPADPLUS_USER_INTERNAL + 1,
        NPPM_INTERNAL_CMDLIST_MODIFIED = NOTEPADPLUS_USER_INTERNAL + 2,
        NPPM_INTERNAL_MACROLIST_MODIFIED = NOTEPADPLUS_USER_INTERNAL + 3,
        NPPM_INTERNAL_PLUGINCMDLIST_MODIFIED = NOTEPADPLUS_USER_INTERNAL + 4,
        NPPM_INTERNAL_CLEARSCINTILLAKEY = NOTEPADPLUS_USER_INTERNAL + 5,
        NPPM_INTERNAL_BINDSCINTILLAKEY = NOTEPADPLUS_USER_INTERNAL + 6,
        NPPM_INTERNAL_SCINTILLAKEYMODIFIED = NOTEPADPLUS_USER_INTERNAL + 7,
        NPPM_INTERNAL_SCINTILLAFINFERCOLLAPSE = NOTEPADPLUS_USER_INTERNAL + 8,
        NPPM_INTERNAL_SCINTILLAFINFERUNCOLLAPSE = NOTEPADPLUS_USER_INTERNAL + 9,
        NPPM_INTERNAL_DISABLEAUTOUPDATE = NOTEPADPLUS_USER_INTERNAL + 10,
        NPPM_INTERNAL_SETTING_HISTORY_SIZE = NOTEPADPLUS_USER_INTERNAL + 11,
        NPPM_INTERNAL_ISTABBARREDUCED = NOTEPADPLUS_USER_INTERNAL + 12,
        NPPM_INTERNAL_ISFOCUSEDTAB = NOTEPADPLUS_USER_INTERNAL + 13,
        NPPM_INTERNAL_GETMENU = NOTEPADPLUS_USER_INTERNAL + 14,
        NPPM_INTERNAL_CLEARINDICATOR = NOTEPADPLUS_USER_INTERNAL + 15,
        NPPM_INTERNAL_SCINTILLAFINFERCOPY = NOTEPADPLUS_USER_INTERNAL + 16,
        NPPM_INTERNAL_SCINTILLAFINFERSELECTALL = NOTEPADPLUS_USER_INTERNAL + 17,
        NPPM_INTERNAL_SETCARETWIDTH = NOTEPADPLUS_USER_INTERNAL + 18,
        NPPM_INTERNAL_SETCARETBLINKRATE = NOTEPADPLUS_USER_INTERNAL + 19,
        NPPM_INTERNAL_CLEARINDICATORTAGMATCH = NOTEPADPLUS_USER_INTERNAL + 20,
        NPPM_INTERNAL_CLEARINDICATORTAGATTR = NOTEPADPLUS_USER_INTERNAL + 21,
        NPPM_INTERNAL_SWITCHVIEWFROMHWND = NOTEPADPLUS_USER_INTERNAL + 22,
        NPPM_INTERNAL_UPDATETITLEBAR = NOTEPADPLUS_USER_INTERNAL + 23,
        NPPM_INTERNAL_CANCEL_FIND_IN_FILES = NOTEPADPLUS_USER_INTERNAL + 24,
        NPPM_INTERNAL_RELOADNATIVELANG = NOTEPADPLUS_USER_INTERNAL + 25,
        NPPM_INTERNAL_PLUGINSHORTCUTMOTIFIED = NOTEPADPLUS_USER_INTERNAL + 26,
        NPPM_INTERNAL_SCINTILLAFINFERCLEARALL = NOTEPADPLUS_USER_INTERNAL + 27,

        NPPM_INTERNAL_SETTING_TAB_REPLCESPACE = NOTEPADPLUS_USER_INTERNAL + 29,
        NPPM_INTERNAL_SETTING_TAB_SIZE = NOTEPADPLUS_USER_INTERNAL + 30,
        NPPM_INTERNAL_RELOADSTYLERS = NOTEPADPLUS_USER_INTERNAL + 31,
        NPPM_INTERNAL_DOCORDERCHANGED = NOTEPADPLUS_USER_INTERNAL + 32,
        NPPM_INTERNAL_SETMULTISELCTION = NOTEPADPLUS_USER_INTERNAL + 33,
        NPPM_INTERNAL_SCINTILLAFINFEROPENALL = NOTEPADPLUS_USER_INTERNAL + 34,
        NPPM_INTERNAL_RECENTFILELIST_UPDATE = NOTEPADPLUS_USER_INTERNAL + 35,
        NPPM_INTERNAL_RECENTFILELIST_SWITCH = NOTEPADPLUS_USER_INTERNAL + 36,
        NPPM_INTERNAL_GETSCINTEDTVIEW = NOTEPADPLUS_USER_INTERNAL + 37,
        NPPM_INTERNAL_ENABLESNAPSHOT = NOTEPADPLUS_USER_INTERNAL + 38,
        NPPM_INTERNAL_SAVECURRENTSESSION = NOTEPADPLUS_USER_INTERNAL + 39,
        NPPM_INTERNAL_FINDINFINDERDLG = NOTEPADPLUS_USER_INTERNAL + 40,
        NPPM_INTERNAL_REMOVEFINDER = NOTEPADPLUS_USER_INTERNAL + 41,
        NPPM_INTERNAL_RELOADSCROLLTOEND = NOTEPADPLUS_USER_INTERNAL + 42,
        NPPM_INTERNAL_FINDKEYCONFLICTS = NOTEPADPLUS_USER_INTERNAL + 43,
        NPPM_INTERNAL_SCROLLBEYONDLASTLINE = NOTEPADPLUS_USER_INTERNAL + 44,
        NPPM_INTERNAL_SETWORDCHARS = NOTEPADPLUS_USER_INTERNAL + 45,
        NPPM_INTERNAL_EXPORTFUNCLISTANDQUIT = NOTEPADPLUS_USER_INTERNAL + 46,
        NPPM_INTERNAL_PRNTANDQUIT = NOTEPADPLUS_USER_INTERNAL + 47,
        NPPM_INTERNAL_SAVEBACKUP = NOTEPADPLUS_USER_INTERNAL + 48,
        NPPM_INTERNAL_STOPMONITORING = NOTEPADPLUS_USER_INTERNAL + 49,
        NPPM_INTERNAL_EDGEBACKGROUND = NOTEPADPLUS_USER_INTERNAL + 50,
        NPPM_INTERNAL_EDGEMULTISETSIZE = NOTEPADPLUS_USER_INTERNAL + 51,
        NPPM_INTERNAL_UPDATECLICKABLELINKS = NOTEPADPLUS_USER_INTERNAL + 52,

        NPPM_INTERNAL_CHECKDOCSTATUS = Constants.NPPMSG + 53,

        NPPM_INTERNAL_ENABLECHECKDOCOPT = Constants.NPPMSG + 54,

        CHECKDOCOPT_NONE = 0,
        CHECKDOCOPT_UPDATESILENTLY = 1,
        CHECKDOCOPT_UPDATEGO2END = 2,

        NPPM_INTERNAL_SETFILENAME = Constants.NPPMSG + 63,

        SCINTILLA_USER = Constants.WM_USER + 2000,

        MACRO_USER = Constants.WM_USER + 4000,
        WM_GETCURRENTMACROSTATUS = MACRO_USER + 01,
        WM_MACRODLGRUNMACRO = MACRO_USER + 02,

        SPLITTER_USER = Constants.WM_USER + 4000,
        WORDSTYLE_USER = Constants.WM_USER + 5000,
        COLOURPOPUP_USER = Constants.WM_USER + 6000,
        BABYGRID_USER = Constants.WM_USER + 7000,

        MENUINDEX_FILE = 0,
        MENUINDEX_EDIT = 1,
        MENUINDEX_SEARCH = 2,
        MENUINDEX_VIEW = 3,
        MENUINDEX_FORMAT = 4,
        MENUINDEX_LANGUAGE = 5,
        MENUINDEX_SETTINGS = 6,
        MENUINDEX_TOOLS = 7,
        MENUINDEX_MACRO = 8,
        MENUINDEX_RUN = 9,
        MENUINDEX_PLUGINS = 10,
		/* --Autogenerated -- end of section automatically generated from resource.h */
	}
}
