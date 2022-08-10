#include "stdafx.h"
#include <aced.h> 
#include <rxregsvc.h> 
#include <string>

// ACHAR is a typedef (made by Autodesk in file AdAChar.h) of wchar_t.So the question is how to convert a char to wchar_t.
// https://stackoverflow.com/questions/1195675/convert-a-char-to-stdstring
// wchar_t to string
// https://stackoverflow.com/questions/27720553/conversion-of-wchar-t-to-string
extern "C" Acad::ErrorStatus removeCommandGroup(const ACHAR* groupName)
{
    acutPrintf(_T("\nAttempting to remove command group: "));
    std::wstring wStringGroupName(groupName);
    std::string stringGroupName(wStringGroupName.begin(), wStringGroupName.end());
    acutPrintf(groupName);
    return acedRegCmds->removeGroup(groupName);
}


extern "C" Acad::ErrorStatus removeCommand(const ACHAR * cmdGroupName, const ACHAR * cmdGlobalName)

{
    acutPrintf(_T("\r\nAttempting to remove command."));
    acutPrintf(_T("\r\nCommand group: "));
    acutPrintf(cmdGroupName);
    acutPrintf(_T("\r\nCommand global name: "));
    acutPrintf(cmdGlobalName);
    return acedRegCmds->removeCmd(cmdGroupName, cmdGlobalName);
}