# Wenn der Befehl dotnet--sshdeploy nicht gefunden wurde, so wurde sshdeploy noch nicht installiert.
# 1. Installation
# dotnet tool install -g dotnet-sshdeploy
# 1.1 Wenn dotnet nicht gefunden wurde, dann dotnet. core runtime installieren

# 2. Setzen der Umgebungsvariable
# Add Environment Variable => "C:\Users\Eriktion\.dotnet\tools\dotnet-sshdeploy.exe"

dotnet-sshdeploy push -c Debug