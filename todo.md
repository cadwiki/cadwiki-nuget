1.) hard coded nuget path
<UsingTask AssemblyFile="C:\Users\{username}\.nuget\packages\msbuild.extension.pack\1.9.1\tools\net40\MSBuild.ExtensionPack.dll" TaskName="AssemblyInfo" />
 
2.) check for non determinsitc assembly info failures *.* in this
<Target Name="autoincrementBuildThisFirstAssemblyInfo" BeforeTargets="Build">
<Exec Command="echo target for auto incrementing AssemblyInfo.cs files" />

3.) skip cmd remove dll import call to avoid clr error when attempting to unload commands
 
4.) interface that implements iExtension
 
5.) interface that adds ribbon tab with a button or buttons pointing to a namespaces and a dll to reload
 
6.) staying up to date with current interop and sdk
 