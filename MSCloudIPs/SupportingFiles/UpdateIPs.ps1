#call the update URLs
$O365Result=(Invoke-webrequest -URI "http://mscloudips.azurewebsites.net/api/office365ips/operation/update").Content
$AzureResult=(Invoke-webrequest -URI "http://mscloudips.azurewebsites.net/api/azureips/operation/update").Content

#write the output
"Last O365 update: " + $O365Result
"Last Azure update: " + $AzureResult