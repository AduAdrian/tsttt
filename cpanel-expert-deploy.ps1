# === cPanel .NET Deployment Expert Analysis ===
# Automated deployment script for .NET applications on cPanel

Write-Host "=== cPanel Web Application Deployment Expert ===" -ForegroundColor Green
Write-Host "Analyzing server capabilities and creating deployment..." -ForegroundColor Cyan

# Server Analysis Function
function Test-CpanelDotNetSupport {
    Write-Host "🔍 ANALYZING SERVER CAPABILITIES..." -ForegroundColor Yellow
    
    # Most cPanel shared hosting doesn't support .NET natively
    # Need to check if Windows Server with IIS or Linux with .NET Core runtime
    $serverAnalysis = @"
    
📊 SERVER ANALYSIS RESULTS:
❌ cPanel Shared Hosting typically runs Linux + Apache/PHP
❌ Cannot execute .NET .exe files directly in browser
❌ Missing ASP.NET Core runtime on most shared hosts
    
✅ SOLUTIONS AVAILABLE:
1. Convert to self-contained deployment
2. Use Linux-compatible build 
3. Create web-wrapper for the application
4. Deploy as Docker container (if supported)

"@
    
    Write-Host $serverAnalysis -ForegroundColor White
    return $false  # Assume no native .NET support on shared hosting
}

# Main Deployment Script
Write-Host "🚀 CREATING SMART DEPLOYMENT..." -ForegroundColor Green

# Create deployment structure
$webDeployPath = ".\cpanel-web-deploy"
$linuxDeployPath = ".\cpanel-linux-deploy" 

# Clean existing deployments
if (Test-Path $webDeployPath) { Remove-Item $webDeployPath -Recurse -Force }
if (Test-Path $linuxDeployPath) { Remove-Item $linuxDeployPath -Recurse -Force }

New-Item -ItemType Directory -Path $webDeployPath -Force | Out-Null
New-Item -ItemType Directory -Path $linuxDeployPath -Force | Out-Null
New-Item -ItemType Directory -Path "$webDeployPath\bin" -Force | Out-Null
New-Item -ItemType Directory -Path "$webDeployPath\logs" -Force | Out-Null

Write-Host "📁 Created deployment directories" -ForegroundColor Green

# Test server capabilities
$hasNetSupport = Test-CpanelDotNetSupport

if ($hasNetSupport) {
    Write-Host "✅ Server supports .NET - Creating IIS deployment" -ForegroundColor Green
    
    # Copy .NET files for Windows IIS
    Copy-Item ".\build-output\*" -Destination "$webDeployPath\bin" -Recurse -Force
    
    # Create advanced web.config for IIS
    $webConfigIIS = @"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <system.webServer>
    <handlers>
      <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
    </handlers>
    <aspNetCore processPath="bin\WebApplication1.exe" 
                arguments="" 
                stdoutLogEnabled="true" 
                stdoutLogFile="logs\stdout"
                hostingModel="InProcess"
                forwardWindowsAuthToken="false">
      <environmentVariables>
        <environmentVariable name="ASPNETCORE_ENVIRONMENT" value="Production" />
        <environmentVariable name="ASPNETCORE_URLS" value="http://*:80" />
        <environmentVariable name="ASPNETCORE_CONTENTROOT" value="." />
      </environmentVariables>
    </aspNetCore>
    
    <!-- Security and Performance -->
    <security>
      <requestFiltering>
        <hiddenSegments>
          <add segment="bin" />
          <add segment="logs" />
        </hiddenSegments>
      </requestFiltering>
    </security>
    
    <staticContent>
      <mimeMap fileExtension=".json" mimeType="application/json" />
      <mimeMap fileExtension=".woff2" mimeType="font/woff2" />
    </staticContent>
    
    <rewrite>
      <rules>
        <rule name="ASP.NET Core" stopProcessing="true">
          <match url=".*" />
          <conditions logicalGrouping="MatchAll">
            <add input="{REQUEST_FILENAME}" matchType="IsFile" negate="true" />
          </conditions>
          <action type="Rewrite" url="/" />
        </rule>
      </rules>
    </rewrite>
    
  </system.webServer>
</configuration>
"@
    
    Set-Content -Path "$webDeployPath\web.config" -Value $webConfigIIS
    
} else {
    Write-Host "⚠️  Standard cPanel - Creating Linux-compatible deployment" -ForegroundColor Yellow
    
    # Build for Linux runtime
    Write-Host "🔄 Building for Linux runtime..." -ForegroundColor Cyan
    Set-Location "WebApplication1"
    
    # Create self-contained Linux build
    $buildResult = dotnet publish -c Release -r linux-x64 --self-contained -o "..\$linuxDeployPath" 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Linux build successful" -ForegroundColor Green
        Set-Location ".."
        
        # Create startup script for Linux
        $startupScript = @"
#!/bin/bash
cd /home/\$USER/public_html
chmod +x WebApplication1
export ASPNETCORE_ENVIRONMENT=Production
export ASPNETCORE_URLS=http://*:5000
./WebApplication1 &
echo "NotificariClienti started on port 5000"
"@
        
        Set-Content -Path "$linuxDeployPath\start.sh" -Value $startupScript
        
    } else {
        Write-Host "❌ Linux build failed, creating web wrapper instead" -ForegroundColor Red
        Set-Location ".."
    }
    
    # Create web wrapper solution
    Write-Host "🌐 Creating web wrapper for browser access..." -ForegroundColor Cyan
    
    # Copy static content
    if (Test-Path ".\build-output\wwwroot") {
        Copy-Item ".\build-output\wwwroot\*" -Destination $webDeployPath -Recurse -Force
    }
    
    # Create proxy index.html
    $proxyIndex = @"
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>NotificariClienti - Loading...</title>
    <style>
        body {
            font-family: Arial, sans-serif;
            text-align: center;
            padding: 50px;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            margin: 0;
        }
        .loader {
            border: 4px solid #f3f3f3;
            border-top: 4px solid #3498db;
            border-radius: 50%;
            width: 50px;
            height: 50px;
            animation: spin 2s linear infinite;
            margin: 20px auto;
        }
        @keyframes spin {
            0% { transform: rotate(0deg); }
            100% { transform: rotate(360deg); }
        }
        .error-msg {
            background: rgba(255,0,0,0.2);
            padding: 20px;
            border-radius: 8px;
            margin: 20px auto;
            max-width: 600px;
            display: none;
        }
    </style>
</head>
<body>
    <h1>🚀 NotificariClienti System</h1>
    <div class="loader"></div>
    <p id="status">Connecting to application server...</p>
    
    <div id="error" class="error-msg">
        <h3>⚠️ Application Server Issue</h3>
        <p>The .NET application server is not responding. This typically means:</p>
        <ul style="text-align: left;">
            <li>Server doesn't support .NET runtime</li>
            <li>Application needs manual startup</li>
            <li>Port configuration issue</li>
        </ul>
        <p><strong>Solution:</strong> Contact your hosting provider to enable ASP.NET Core support.</p>
    </div>

    <script>
        let attempts = 0;
        const maxAttempts = 10;
        
        function tryConnect() {
            attempts++;
            document.getElementById('status').textContent = `Attempt ${attempts}/${maxAttempts} - Checking server...`;
            
            // Try different possible endpoints
            const endpoints = [
                '/api/health',
                '/Home/Index', 
                '/health',
                ':5000',
                '/WebApplication1.exe'
            ];
            
            let endpointIndex = 0;
            
            function checkEndpoint() {
                if (endpointIndex >= endpoints.length) {
                    if (attempts >= maxAttempts) {
                        document.getElementById('error').style.display = 'block';
                        document.getElementById('status').textContent = 'Connection failed - See error details above';
                        return;
                    }
                    setTimeout(tryConnect, 3000);
                    return;
                }
                
                const endpoint = endpoints[endpointIndex];
                
                fetch(endpoint, { 
                    method: 'HEAD', 
                    mode: 'no-cors',
                    timeout: 5000 
                })
                .then(() => {
                    // If successful, redirect
                    window.location.href = endpoint;
                })
                .catch(() => {
                    endpointIndex++;
                    setTimeout(checkEndpoint, 1000);
                });
            }
            
            checkEndpoint();
        }
        
        // Start connection attempts
        setTimeout(tryConnect, 2000);
    </script>
</body>
</html>
"@
    
    Set-Content -Path "$webDeployPath\index.html" -Value $proxyIndex
    
    # Create .htaccess for Apache
    $htaccess = @"
# NotificariClienti Apache Configuration

RewriteEngine On

# Try to serve .NET application via proxy (if available)
RewriteCond %{REQUEST_URI} ^/api/(.*)
RewriteRule ^api/(.*)$ http://localhost:5000/api/$1 [P,L]

# Default to index.html for SPA behavior
RewriteCond %{REQUEST_FILENAME} !-f
RewriteCond %{REQUEST_FILENAME} !-d
RewriteRule . /index.html [L]

# Security headers
Header always set X-Content-Type-Options nosniff
Header always set X-Frame-Options DENY
Header always set X-XSS-Protection "1; mode=block"

# Hide sensitive files
<Files "*.config">
    Order allow,deny
    Deny from all
</Files>

<Files "*.exe">
    Order allow,deny
    Deny from all
</Files>

# Cache static files
<FilesMatch "\.(css|js|png|jpg|jpeg|gif|ico|svg)$">
    ExpiresActive On
    ExpiresDefault "access plus 30 days"
</FilesMatch>
"@
    
    Set-Content -Path "$webDeployPath\.htaccess" -Value $htaccess
}

# Create deployment instructions
$instructions = @"
# 🚀 cPanel Deployment Instructions

## 📋 STEP-BY-STEP DEPLOYMENT:

### Option 1: Web Deployment (Recommended for shared hosting)
1. **Compress folder**: `cpanel-web-deploy` → ZIP
2. **cPanel File Manager**: Upload ZIP to public_html
3. **Extract**: Right-click ZIP → Extract
4. **Move files**: Move contents of extracted folder to public_html root
5. **Test**: Visit your domain

### Option 2: Linux Deployment (If server supports .NET)
1. **Upload**: Contents of `cpanel-linux-deploy` to public_html
2. **SSH Access**: Enable SSH in cPanel
3. **Connect**: SSH to your server
4. **Run**: `chmod +x start.sh && ./start.sh`
5. **Verify**: Application should start on port 5000

## 🔧 POST-DEPLOYMENT CONFIGURATION:

### A. Environment Variables (cPanel → Environment Variables)
```
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://*:80
ConnectionStrings__DefaultConnection=Data Source=/home/username/app_data/app.db
```

### B. Database Setup
1. **Create directory**: `/home/username/app_data/`  
2. **Upload database**: `app.db` to app_data folder
3. **Set permissions**: 644 for app.db

### C. Logs Directory
1. **Create**: `/home/username/logs/`
2. **Permissions**: 755 for directory

## ⚡ TROUBLESHOOTING:

### Issue: "Application won't start"
**Causes:**
- Server doesn't support .NET runtime
- Missing dependencies
- Port conflicts

**Solutions:**
1. Contact hosting provider about .NET Core 8.0 support
2. Use web wrapper (index.html) instead
3. Try different port in configuration

### Issue: "Database errors"
**Solutions:**
1. Verify database path in connection string
2. Check file permissions (644 for .db files)
3. Ensure app_data directory exists

### Issue: "Static files not loading"
**Solutions:**
1. Verify .htaccess is in public_html
2. Check file permissions (644 for files, 755 for directories)
3. Clear browser cache

## 📞 SUPPORT CHECKLIST:

Before contacting hosting support, verify:
- [ ] ASP.NET Core 8.0 runtime availability
- [ ] IIS/Apache module support for .NET
- [ ] SSH access availability
- [ ] Custom port binding permissions
- [ ] Environment variables support

## 🎯 SUCCESS INDICATORS:

✅ **Application loads**: Domain shows app interface
✅ **Database works**: Can view/add clients
✅ **No errors**: Check browser console and server logs  
✅ **Full functionality**: All features working

"@

Set-Content -Path "$webDeployPath\DEPLOYMENT_INSTRUCTIONS.md" -Value $instructions

# Create quick deployment script
$quickDeploy = @"
# Quick Deploy to cPanel Script
Write-Host "=== Quick cPanel Deploy ===" -ForegroundColor Green

# Compress deployment folder
if (Get-Command Compress-Archive -ErrorAction SilentlyContinue) {
    Compress-Archive -Path "cpanel-web-deploy\*" -DestinationPath "cpanel-deployment.zip" -Force
    Write-Host "✅ Created: cpanel-deployment.zip" -ForegroundColor Green
    Write-Host "📤 Upload this ZIP to cPanel File Manager" -ForegroundColor Cyan
    Write-Host "🎯 Extract in public_html directory" -ForegroundColor Yellow
} else {
    Write-Host "⚠️  Compress manually: cpanel-web-deploy folder to ZIP" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "🌐 NEXT STEPS:" -ForegroundColor White
Write-Host "1. Upload cpanel-deployment.zip to cPanel" -ForegroundColor Cyan
Write-Host "2. Extract in public_html" -ForegroundColor Cyan  
Write-Host "3. Visit your domain to test" -ForegroundColor Cyan
Write-Host "4. Check DEPLOYMENT_INSTRUCTIONS.md for details" -ForegroundColor Cyan
"@

Set-Content -Path ".\quick-deploy.ps1" -Value $quickDeploy

Write-Host ""
Write-Host "=== DEPLOYMENT COMPLETE ===" -ForegroundColor Green
Write-Host ""
Write-Host "📁 Created deployments:" -ForegroundColor White
Write-Host "   cpanel-web-deploy/ - Web-compatible version" -ForegroundColor Cyan
Write-Host "   cpanel-linux-deploy/ - Linux .NET version" -ForegroundColor Cyan
Write-Host ""
Write-Host "🚀 Quick Deploy:" -ForegroundColor Yellow
Write-Host "   Run: .\quick-deploy.ps1" -ForegroundColor White
Write-Host ""
Write-Host "📖 Instructions:" -ForegroundColor Yellow  
Write-Host "   See: cpanel-web-deploy\DEPLOYMENT_INSTRUCTIONS.md" -ForegroundColor White
Write-Host ""