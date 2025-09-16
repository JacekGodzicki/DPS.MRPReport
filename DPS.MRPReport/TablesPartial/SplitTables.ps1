$ErrorActionPreference = 'STOP'

function RemoveFile($DirPath) {
	$PathWithExtension = $DirPath + "\*.xml"
	Remove-Item $PathWithExtension
}

function RemoveAttributes($Template) {
    $null = $Template.module.RemoveAttribute("name")
    $null = $Template.module.RemoveAttribute("namespace")
    $null = $Template.module.RemoveAttribute("versionName")
    $null = $Template.module.RemoveAttribute("versionNumber")
}

function AddTable($Template, $XmlTable, $SavePath) {
    RemoveAttributes($Template)
    $nodeToImport = $Template.ImportNode($XmlTable, $true)
    $null = $Template.module.AppendChild($nodeToImport)
    $Template.Save(-join($SavePath, "\", $XmlTable.tablename, ".xml"))
}

function AddTable($Template, $XmlSubRow, $SavePath) {
    RemoveAttributes($Template)
    $nodeToImport = $Template.ImportNode($XmlSubRow, $true)
    $null = $Template.module.AppendChild($nodeToImport)
    $Template.Save(-join($SavePath, "\", $XmlSubRow.name, ".xml"))
}

function AddEnums($Template, $BusinessXml, $SavePath) {
    RemoveAttributes($Template)
    foreach($enum in $BusinessXml.module.enum) {
        $nodeToImport = $Template.ImportNode($enum, $true)
        $null = $Template.module.AppendChild($nodeToImport)
    }
    $Template.Save(-join($SavePath, "\", "_Enums.xml"))
}

function AddInterfaces($Template, $BusinessXml, $SavePath) {
    RemoveAttributes($Template)
    foreach($interface in $BusinessXml.module.interface) {
        $nodeToImport = $Template.ImportNode($interface, $true)
        $null = $Template.module.AppendChild($nodeToImport)
    }
    $Template.Save(-join($SavePath, "\", "_Interfaces.xml"))
}

function AddUsingsImports($Template, $BusinessXml, $SavePath) {
    foreach($node in $BusinessXml.module.ChildNodes) {
        if(($node.Name -eq "using") -or ($node.Name -eq "import")) {
            $nodeToImport = $Template.ImportNode($node, $true)
            $null = $Template.module.AppendChild($nodeToImport)
        }
    }
    $Template.Save(-join($SavePath, "\", "_Using.xml"))
}

Try
{
	[string]$scriptRunLocation = split-path -parent $MyInvocation.MyCommand.Definition
	Write-Host "scriptRunLocation " $scriptRunLocation
	$files = Get-ChildItem (-join($scriptRunLocation, "\*business.xml"))
    foreach($file in $files) {
        [xml]$BusinessXml = Get-Content $file
        $ModulePath = -join($scriptRunLocation, "\", $BusinessXml.module.name)
        if(Test-Path -Path $ModulePath) {
            RemoveFile($ModulePath)
        } else {
            New-Item -Path $scriptRunLocation -Name $BusinessXml.module.name -ItemType "directory"
        }
        [Xml]$TemplateXml = $BusinessXml.Clone();
        while($TemplateXml.module.ChildNodes.Count -gt 0) {
            $null = $TemplateXml.module.RemoveChild($TemplateXml.module.ChildNodes[0])
        }
        AddEnums $TemplateXml.Clone() $BusinessXml $ModulePath
        AddInterfaces $TemplateXml.Clone() $BusinessXml $ModulePath
        foreach($Table in $BusinessXml.module.table) {
            AddTable $TemplateXml.Clone() $Table $ModulePath
        }
        foreach($Subrow in $BusinessXml.module.subrow) {
            AddTable $TemplateXml.Clone() $Subrow $ModulePath
        }
        AddUsingsImports $TemplateXml.Clone() $BusinessXml $ModulePath
    }

	Write-Host "end -----------------------------------------------"
	exit 0
}
Catch
{
	Write-Host $Error[0]
	throw 1
}