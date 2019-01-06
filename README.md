[![Build status](https://ci.appveyor.com/api/projects/status/e1ia1r0arwcdl8wb?svg=true)](https://ci.appveyor.com/project/gr4b4z/azure-helper-q2waa)


# Azure-TF-Helper
AzureTF Helper is a little cli tool that helps interacting with terraform state and azure

### Table of Content
  * [Download](#download)
    + [Linux](#linux)
    + [macOS](#macos)
    + [Windows](#windows)
  * [Commands](#commands)
  * [How to](#how-to)

 
  * [License](#license)


## download
### Linux
```sh
gr4b4z@pc1:~/home$ curl -Loa AzureHelper https://github.com/gr4b4z/azure-helper/releases/download/1.0.3/AzureHelper
gr4b4z@pc1:~/home$ chmod +x AzureHelper
```
### Windows
```powershell
PS C:\Users\gr4b4z\tmp> [Net.ServicePointManager]::SecurityProtocol = "tls12, tls11, tls"
PS C:\Users\gr4b4z\tmp> Invoke-WebRequest https://github.com/gr4b4z/azure-helper/releases/download/1.0.3/AzureHelper -OutFile AzureHelper
```
## How to
```powershell
Commands:
  download  Download file/folders from Azure storage
  setsite   Set azure website (temporary solution)
  tf        Get terraform data
  upload    Upload app or folder to Azure storage / Upload Azure function 
```

## How to

**Get remote TF state**
```powershell
 .\AzureHelper.exe tf attribute download [remote-container-name] [remote-file-name] terraform.tfstate --c [Azure blob storage connection string]
```

**Get attribute from terraform.tfstate file**
```powershell
 .\AzureHelper.exe tf attribute azurerm_servicebus_topic.test.name -f terraform.tfstate #looking for attribute name from the azurerm_servicebus_topic rersource
 .\AzureHelper.exe tf attribute azurerm_servicebus_topic.test.name  #when no tfstate file, it search throught all *.tfstate files in the current directory
```

**Replace key in a file base on terraform.tfstate **
```html
 <html>
 <title>@#terraform.azurerm_servicebus_topic.test.name</title>
 </html>
```
@#terraform. is a default replacement prefix. 

```powershell
 .\AzureHelper.exe tf replace index.html #replace all tags keys started from @#terraform. to the values from terraform.tfstate
```





## License
This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details
