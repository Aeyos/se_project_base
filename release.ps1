# Remove previous build
Remove-Item -Recurse "../$(Split-Path -Path $pwd -Leaf)_Release/"

# Copy all cs scripts to folder
Copy-Item *.cs "SEModFiles/Data/Scripts/$(Split-Path -Path $pwd -Leaf)/"

# Copy folder to parent with name_Release
Copy-Item -Recurse SEModFiles "../$(Split-Path -Path $pwd -Leaf)_Release/"

Remove-Item -Recurse "../$(Split-Path -Path $pwd -Leaf)_Release/Models/SourceModels"
Remove-Item -Recurse "../$(Split-Path -Path $pwd -Leaf)_Release/Models/Utilities"