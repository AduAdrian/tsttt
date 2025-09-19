Write-Host "=== cPanel Web Application Deployment Expert ===" -ForegroundColor Green
Write-Host "Analyzing server capabilities and creating deployment..." -ForegroundColor Cyan

# Clean existing deployments
$webDeployPath = ".\cpanel-web-deploy"
$linuxDeployPath = ".\cpanel-linux-deploy" 

if (Test-Path $webDeployPath) { Remove-Item $webDeployPath -Recurse -Force }
if (Test-Path $linuxDeployPath) { Remove-Item $linuxDeployPath -Recurse -Force }

New-Item -ItemType Directory -Path $webDeployPath -Force | Out-Null
New-Item -ItemType Directory -Path "$webDeployPath\bin" -Force | Out-Null
New-Item -ItemType Directory -Path "$webDeployPath\logs" -Force | Out-Null

Write-Host "📁 Created deployment directories" -ForegroundColor Green

Write-Host "⚠️  ANALYSIS: cPanel shared hosting typically doesn't support .NET executables" -ForegroundColor Yellow
Write-Host "🔄 Creating web-compatible deployment..." -ForegroundColor Cyan

# Copy static content if available
if (Test-Path ".\build-output\wwwroot") {
    Copy-Item ".\build-output\wwwroot\*" -Destination $webDeployPath -Recurse -Force
    Write-Host "✅ Copied static content" -ForegroundColor Green
}

# Copy application files to bin directory
Copy-Item ".\build-output\*" -Destination "$webDeployPath\bin" -Recurse -Force
Write-Host "✅ Copied application files to bin" -ForegroundColor Green

# Create web.config for IIS/AspNetCore module
$webConfig = @'
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
                hostingModel="InProcess">
      <environmentVariables>
        <environmentVariable name="ASPNETCORE_ENVIRONMENT" value="Production" />
        <environmentVariable name="ASPNETCORE_URLS" value="http://*:80" />
      </environmentVariables>
    </aspNetCore>
    <security>
      <requestFiltering>
        <hiddenSegments>
          <add segment="bin" />
          <add segment="logs" />
        </hiddenSegments>
      </requestFiltering>
    </security>
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
'@

Set-Content -Path "$webDeployPath\web.config" -Value $webConfig
Write-Host "✅ Created web.config" -ForegroundColor Green

# Create fallback index.html for servers without .NET support
$indexHtml = @'
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>NotificariClienti - System</title>
    <style>
        body {
            font-family: 'Segoe UI', Arial, sans-serif;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            margin: 0;
            padding: 50px;
            text-align: center;
        }
        .container {
            max-width: 800px;
            margin: 0 auto;
            background: rgba(255,255,255,0.1);
            padding: 40px;
            border-radius: 15px;
            backdrop-filter: blur(10px);
        }
        .loader {
            border: 4px solid rgba(255,255,255,0.3);
            border-top: 4px solid #fff;
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
        .status { margin: 20px 0; font-size: 18px; }
        .error-info {
            background: rgba(255,0,0,0.2);
            padding: 20px;
            border-radius: 8px;
            margin: 20px 0;
            text-align: left;
        }
        .success-info {
            background: rgba(0,255,0,0.2);
            padding: 20px;
            border-radius: 8px;
            margin: 20px 0;
            text-align: left;
        }
        button {
            background: #4CAF50;
            color: white;
            border: none;
            padding: 12px 24px;
            border-radius: 5px;
            cursor: pointer;
            font-size: 16px;
            margin: 10px;
        }
        button:hover { background: #45a049; }
    </style>
</head>
<body>
    <div class="container">
        <h1>🚀 NotificariClienti System</h1>
        <div class="loader" id="loader"></div>
        <div class="status" id="status">Initializing application...</div>
        
        <div id="error-info" class="error-info" style="display:none;">
            <h3>⚠️ Server Configuration Required</h3>
            <p><strong>Issue:</strong> This server doesn't support .NET applications natively.</p>
            <h4>Solutions:</h4>
            <ol>
                <li><strong>Contact Hosting Provider:</strong> Ask about ASP.NET Core 8.0 support</li>
                <li><strong>Enable .NET Module:</strong> Install AspNetCoreModuleV2 in IIS</li>
                <li><strong>Alternative Hosting:</strong> Use Windows Server with IIS or Azure App Service</li>
            </ol>
            <p><strong>Technical Details:</strong></p>
            <ul>
                <li>Application: ASP.NET Core 8.0</li>
                <li>Required Module: AspNetCoreModuleV2</li>
                <li>Runtime: .NET 8.0</li>
            </ul>
        </div>
        
        <div id="success-info" class="success-info" style="display:none;">
            <h3>✅ Application Ready</h3>
            <p>NotificariClienti is now running successfully!</p>
        </div>
        
        <div id="actions" style="display:none;">
            <button onclick="location.reload()">🔄 Retry Connection</button>
            <button onclick="window.open('/bin/WebApplication1.exe', '_self')">🚀 Launch App Direct</button>
        </div>
    </div>

    <script>
        let connectionAttempts = 0;
        const maxAttempts = 5;
        
        function updateStatus(message) {
            document.getElementById('status').textContent = message;
        }
        
        function showError() {
            document.getElementById('loader').style.display = 'none';
            document.getElementById('error-info').style.display = 'block';
            document.getElementById('actions').style.display = 'block';
            updateStatus('Configuration required - see details below');
        }
        
        function showSuccess() {
            document.getElementById('loader').style.display = 'none';
            document.getElementById('success-info').style.display = 'block';
            updateStatus('Application loaded successfully!');
        }
        
        function testAppConnection() {
            connectionAttempts++;
            updateStatus(`Testing connection... (${connectionAttempts}/${maxAttempts})`);
            
            // Test multiple endpoints
            const testEndpoints = ['/', '/Home', '/api/health', '/health'];
            let endpointIndex = 0;
            
            function testEndpoint() {
                if (endpointIndex >= testEndpoints.length) {
                    if (connectionAttempts >= maxAttempts) {
                        showError();
                        return;
                    }
                    setTimeout(testAppConnection, 2000);
                    return;
                }
                
                const endpoint = testEndpoints[endpointIndex];
                
                fetch(endpoint, {
                    method: 'HEAD',
                    mode: 'no-cors'
                }).then(response => {
                    showSuccess();
                    setTimeout(() => {
                        window.location.href = endpoint;
                    }, 2000);
                }).catch(() => {
                    endpointIndex++;
                    setTimeout(testEndpoint, 500);
                });
            }
            
            testEndpoint();
        }
        
        // Start testing after page load
        setTimeout(testAppConnection, 2000);
    </script>
</body>
</html>
'@

Set-Content -Path "$webDeployPath\index.html" -Value $indexHtml
Write-Host "✅ Created fallback index.html" -ForegroundColor Green

# Create .htaccess for Apache servers
$htaccess = @'
# NotificariClienti Apache Configuration
RewriteEngine On

# Try to serve application files
RewriteCond %{REQUEST_FILENAME} !-f
RewriteCond %{REQUEST_FILENAME} !-d
RewriteRule ^api/(.*)$ /bin/WebApplication1.exe [L]

# Default to index.html
RewriteCond %{REQUEST_FILENAME} !-f
RewriteCond %{REQUEST_FILENAME} !-d
RewriteRule . /index.html [L]

# Security
Header always set X-Content-Type-Options nosniff
Header always set X-Frame-Options DENY

# Hide sensitive files
<Files "*.exe">
    Order allow,deny
    Deny from all
</Files>

<Files "*.config">
    Order allow,deny  
    Deny from all
</Files>

# Cache static files
<FilesMatch "\.(css|js|png|jpg|jpeg|gif|ico|svg)$">
    ExpiresActive On
    ExpiresDefault "access plus 30 days"
</FilesMatch>
'@

Set-Content -Path "$webDeployPath\.htaccess" -Value $htaccess
Write-Host "✅ Created .htaccess" -ForegroundColor Green

# Create deployment instructions
$deployInstructions = @'
# 🚀 COMPLETE cPanel DEPLOYMENT GUIDE

## QUICK DEPLOYMENT STEPS:

### 1. Upload to cPanel
1. Compress `cpanel-web-deploy` folder to ZIP
2. Login to cPanel → File Manager  
3. Navigate to `public_html` directory
4. Upload the ZIP file
5. Right-click ZIP → Extract
6. Move extracted contents to public_html root
7. Delete ZIP file

### 2. Set Permissions
- Files: 644 (read/write for owner, read for group/others)
- Directories: 755 (read/write/execute for owner, read/execute for others)

### 3. Test Application
- Visit your domain
- Check if application loads
- Monitor for any errors

## TROUBLESHOOTING:

### If Application Doesn't Load:
1. **Check Error Logs**: cPanel → Error Logs
2. **Verify .NET Support**: Contact hosting provider
3. **Test Direct Access**: Try accessing `/bin/WebApplication1.exe`

### Common Issues:

**Issue**: "HTTP 500 Error"
- **Cause**: Server doesn't support .NET
- **Solution**: Contact hosting provider for ASP.NET Core support

**Issue**: "File Not Found" 
- **Cause**: Incorrect file permissions or path
- **Solution**: Check file permissions and directory structure

**Issue**: "Module Not Found"
- **Cause**: AspNetCoreModule not installed
- **Solution**: Ask hosting provider to install IIS AspNetCore module

## SERVER REQUIREMENTS:

✅ **Minimum Requirements:**
- Windows Server with IIS + AspNetCore Module
- OR Linux with .NET 8.0 runtime
- OR Shared hosting with .NET support

❌ **Won't Work On:**
- Standard PHP-only shared hosting
- Servers without .NET runtime
- Hostings that block executable files

## ALTERNATIVE SOLUTIONS:

If current hosting doesn't support .NET:

1. **Upgrade Hosting Plan**: Look for ".NET hosting" or "Windows hosting"
2. **Change Host**: Use hosting providers that support ASP.NET Core
3. **Cloud Deployment**: Consider Azure App Service or AWS Elastic Beanstalk
4. **VPS/Dedicated**: Get server with full control

## SUCCESS INDICATORS:

✅ Application homepage loads
✅ Can navigate through menus
✅ Database operations work  
✅ No console errors
✅ All features functional

Contact hosting support if you need help enabling .NET support!
'@

Set-Content -Path "$webDeployPath\DEPLOYMENT_GUIDE.txt" -Value $deployInstructions
Write-Host "✅ Created deployment guide" -ForegroundColor Green

Write-Host ""
Write-Host "=== DEPLOYMENT READY! ===" -ForegroundColor Green
Write-Host ""
Write-Host "📁 Files created in: cpanel-web-deploy/" -ForegroundColor Cyan
Write-Host "   ├── index.html (fallback page)" -ForegroundColor White
Write-Host "   ├── web.config (IIS configuration)" -ForegroundColor White  
Write-Host "   ├── .htaccess (Apache configuration)" -ForegroundColor White
Write-Host "   ├── bin/ (application files)" -ForegroundColor White
Write-Host "   ├── wwwroot/ (static content)" -ForegroundColor White
Write-Host "   └── DEPLOYMENT_GUIDE.txt" -ForegroundColor White
Write-Host ""
Write-Host "🚀 NEXT STEPS:" -ForegroundColor Yellow
Write-Host "1. Compress 'cpanel-web-deploy' folder to ZIP" -ForegroundColor White
Write-Host "2. Upload ZIP to cPanel File Manager → public_html" -ForegroundColor White
Write-Host "3. Extract ZIP and move contents to public_html root" -ForegroundColor White
Write-Host "4. Visit your domain to test" -ForegroundColor White
Write-Host ""
Write-Host "⚠️  NOTE: If app doesn't load, check DEPLOYMENT_GUIDE.txt for troubleshooting" -ForegroundColor Yellow