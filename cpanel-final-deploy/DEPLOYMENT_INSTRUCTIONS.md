# 🚀 COMPLETE cPanel Deployment Guide

## IMMEDIATE DEPLOYMENT STEPS:

### 1. Prepare Files
✅ DONE: Files are ready in `cpanel-final-deploy` folder

### 2. Upload to cPanel
1. **Compress folder**: Right-click `cpanel-final-deploy` → Send to → Compressed folder
2. **Login to cPanel**: Go to your hosting control panel
3. **Open File Manager**: Navigate to File Manager
4. **Go to public_html**: Click on public_html directory
5. **Upload ZIP**: Click Upload, select your ZIP file
6. **Extract**: Right-click uploaded ZIP → Extract
7. **Move files**: Move ALL contents from extracted folder to public_html root
8. **Delete ZIP**: Clean up by deleting the ZIP file

### 3. Set Permissions (Important!)
In cPanel File Manager:
- **Select all files**: Ctrl+A or Select All
- **Permissions**: Right-click → Change Permissions
- **Files**: Set to 644 (rw-r--r--)  
- **Directories**: Set to 755 (rwxr-xr-x)
- **Apply**: Click Change Permissions

### 4. Test Application  
- **Visit your domain**: Open browser, go to yourdomain.com
- **Check loading**: Should show "NotificariClienti - Loading" page
- **Wait for redirect**: App will test connection and redirect

## WHAT EACH FILE DOES:

📁 **bin/**: Contains all .NET application files
📄 **web.config**: IIS configuration for .NET apps
📄 **index.html**: Fallback page with connection testing  
📄 **.htaccess**: Apache configuration and security
📄 **wwwroot/**: Static files (CSS, JS, images)

## TROUBLESHOOTING:

### ❌ "Application won't start"
**Symptoms**: Gets stuck on loading page, shows error
**Causes**: 
- Server doesn't support .NET
- Missing AspNetCore module
- Wrong permissions

**Solutions**:
1. **Contact hosting support**: Ask for "ASP.NET Core 8.0 support"
2. **Check server type**: Ensure it's Windows Server with IIS  
3. **Verify permissions**: Files 644, directories 755
4. **Try different hosting**: Look for ".NET hosting" providers

### ❌ "HTTP 500 Error"
**Cause**: Server configuration issue
**Solution**: Check cPanel Error Logs, contact support

### ❌ "Files not found"  
**Cause**: Incorrect upload or permissions
**Solution**: Re-upload, check file structure

### ✅ "App loads but features broken"
**Cause**: Database or API issues  
**Solution**: Check database path, environment variables

## SERVER REQUIREMENTS:

### ✅ **Will Work On:**
- Windows Server + IIS + AspNetCoreModuleV2
- Linux server + .NET 8.0 runtime  
- Hosting providers with ".NET support"
- Cloud platforms (Azure, AWS)

### ❌ **Won't Work On:**  
- Standard PHP-only shared hosting
- Servers without .NET runtime
- Basic Apache/cPanel without .NET modules

## HOSTING RECOMMENDATIONS:

If current hosting doesn't work:

1. **Upgrade current host**: Ask about Windows/.NET plans
2. **Switch providers**: Look for:
   - "ASP.NET hosting"
   - "Windows hosting" 
   - ".NET Core support"
3. **Cloud hosting**: Azure App Service, AWS Elastic Beanstalk
4. **VPS/Dedicated**: Full control over server configuration

## SUCCESS INDICATORS:

✅ **Domain loads** without errors  
✅ **Can navigate** through app menus
✅ **Database works** (can view/add clients)
✅ **No console errors** in browser F12
✅ **All features functional**

## QUICK HELP COMMANDS:

**Check if .NET is supported**:
Contact hosting support and ask: "Do you support ASP.NET Core 8.0 applications with AspNetCoreModuleV2?"

**Alternative test**:
Try accessing: `yourdomain.com/bin/WebApplication1.exe` 
- If it downloads: Server doesn't support .NET
- If it shows app: Server might support .NET

---

**🎯 Goal**: Get NotificariClienti running on your domain
**📞 Support**: Contact hosting provider if app won't start
**⏰ Timeline**: Should work within 15 minutes on compatible hosting