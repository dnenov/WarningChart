### How to create an installer using msi cmd

Run the below from a command prompt open in the same path as the installer file "ArchilizerWarchart_Installer.wxs".

`wix build WarningChart_Installer.wxs -out ArchilizerWarchart_Installer.msi`

No output is a good thing. The command should just run and you'll get a fresh .msi file.

>!Note
> For different configurations, make sure you point to the correct source location of the installation files "C:\Users\DeyanNenov\AppData\Roaming\Autodesk\ApplicationPlugins"
> This needs to be hard-coded because of how stupid Wix is
