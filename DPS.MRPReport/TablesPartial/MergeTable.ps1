$ErrorActionPreference = 'STOP'

$EnumFileName = "\_Enums.xml"
$InterfaceFileName = "\_Interfaces.xml"
$UsingFileName = "\_Using.xml"

function RemoveFile($DirPath) {
	$PathWithExtension = $DirPath + "\*.business.xml"
	Remove-Item $PathWithExtension
	Write-Host("Removed files")
}

function AddCommentLine($Xml, $Comment) {
	$emptyLine = $xml.CreateComment(" ")
	$null = $Xml.module.AppendChild($emptyLine)
	$commentLine = $xml.CreateComment($Comment)
	$null = $Xml.module.AppendChild($commentLine)
}

function AddTable($Xml, $file) {
	[xml]$tableXml = Get-Content $file
	foreach($table in $tableXml.module.table) {
		AddCommentLine $Xml $table.name
		$nodeToImport = $Xml.ImportNode($table, $true)
		$null = $Xml.module.AppendChild($nodeToImport)
	}
}

function AddSubrow($Xml, $file) {
	[xml]$subrowXml = Get-Content $file
	foreach($subrow in $subrowXml.module.subrow) {
		AddCommentLine $Xml $subrow.name
		$nodeToImport = $Xml.ImportNode($subrow, $true)
		$null = $Xml.module.AppendChild($nodeToImport)
	}
}

function GeTableVersionByModuleName($BuildPropsXml, $ModuleName) {
	foreach($PropertyGroup in $BuildPropsXml.Project.PropertyGroup) {
		if($PropertyGroup.Label -eq "PreBuildScript") {
			foreach($VersionNumber in $PropertyGroup.VersionNumber) {
				if($VersionNumber.Label -eq $ModuleName) {
					return $VersionNumber.InnerText
				}
			}
		}
	}
}

function CreateVersionAttribute($XmlToModify, $CurrentTableVersion) {
	$Atrib = $XmlToModify.CreateAttribute("versionNumber")
	$Atrib.Value = $CurrentTableVersion
	$XmlToModify.module.Attributes.Append($Atrib)
}

Try
{
	[string]$scriptRunLocation = split-path -parent $MyInvocation.MyCommand.Definition
	Write-Host "scriptRunLocation " $scriptRunLocation
	$mainDir = Get-ChildItem $scriptRunLocation | Where-Object{$_.PSISContainer}
	$SolutionDirObject = ($mainDir.Parent.Parent.Parent | Select-Object -ExpandProperty "FullName")
	$DirectoryBuildPropsFile = Get-ChildItem (-join($SolutionDirObject, "\Directory.Build.props"))
	[xml]$DirectoryBuildPropsXml = Get-Content $DirectoryBuildPropsFile
	foreach ($d in $mainDir){
		$usingFile = Get-ChildItem (-join($d.PSPath, $UsingFileName))
		[xml]$xml = Get-Content $usingFile
		if(-not $xml.module.HasAttribute("versionNumber")) {
			$TableVersion = GeTableVersionByModuleName $DirectoryBuildPropsXml $xml.module.name
			CreateVersionAttribute $xml $TableVersion
		}
		$enumFilePath = -join($d.PSPath, $EnumFileName)
		if(Test-Path -Path $enumFilePath) {
			$enumFile = Get-ChildItem $enumFilePath
			AddCommentLine $xml "Enums"
			[xml]$enumsXml = Get-Content $enumFile
			foreach($enum in $enumsXml.module.enum) {
				$nodeToImport = $xml.ImportNode($enum, $true)
				$null = $xml.module.AppendChild($nodeToImport)
			}
		}
		$interfaceFilePath = -join($d.PSPath, $InterfaceFileName)
		if(Test-Path -Path $interfaceFilePath) {
			$interfaceFile = Get-ChildItem $interfaceFilePath
			AddCommentLine $xml "Interfaces"
			[xml]$interfacesXml = Get-Content $interfaceFile
			foreach($interface in $interfacesXml.module.interface) {
				$nodeToImport = $xml.ImportNode($interface, $true)
				$null = $xml.module.AppendChild($nodeToImport)
			}
		}
		$files = Get-ChildItem (-join($d.PSPath, "\*.xml"))
		foreach($file in $files) {
            $flagEnumFile = ((Get-Item $file.PSPath).Name -eq $EnumFileName.Replace("\", ""))
            $flagUsingFile = ((Get-Item $file.PSPath).Name -eq $UsingFileName.Replace("\", ""))
            $flagInterfaceFile = ((Get-Item $file.PSPath).Name -eq $InterfaceFileName.Replace("\", ""))
			if(-not ($flagEnumFile -or $flagUsingFile -or $flagInterfaceFile) ) {
				AddTable $xml $file
				AddSubrow $xml $file
			}
		}
		$ProjectPath = -join($SolutionDirObject, "\", $xml.module.namespace)
		if(Test-Path -Path $ProjectPath) {
			RemoveFile $ProjectPath
			$xml.Save(-join($ProjectPath, "\", $d.Name, ".business.xml"))
			Write-Host(-join("Saving ", $d.Name, ".business.xml"))
		}
	}
	Write-Host "end -----------------------------------------------"
	exit 0
}
Catch
{
	Write-Host $Error[0]
	throw 1
}