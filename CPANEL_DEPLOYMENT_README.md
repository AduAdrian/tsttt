# cPanel Git Deployment Guide for NotificariClienti

## 🚀 How to Deploy from Git in cPanel

### Method 1: Automatic Deployment (Push Deployment)
When you push changes to the Git repository, cPanel automatically deploys them:

1. **Make changes locally** and commit them
2. **Push to repository**: `git push origin master`
3. **cPanel auto-deploys** using the `.cpanel.yml` configuration file

### Method 2: Manual Deployment (Pull Deployment) 
Use cPanel interface to manually trigger deployment:

1. **cPanel → Git™ Version Control**
2. Click **"Update from Remote"** to pull latest changes
3. Click **"Deploy HEAD Commit"** to deploy to production

## 📋 What the .cpanel.yml File Does

The `.cpanel.yml` file automatically:

✅ **Copies application files** to `/public_html/`  
✅ **Moves database** to `/app_data/` (secure location)  
✅ **Creates log directory** at `/logs/`  
✅ **Sets proper permissions** (644 for files, 755 for directories)  
✅ **Creates .htaccess** with security headers and URL rewriting  
✅ **Copies static content** from wwwroot to public_html root  

## 🔧 cPanel Setup Requirements

### Step 1: Enable Git in cPanel
1. **cPanel → Git™ Version Control**
2. **Create Repository** or **Clone existing**:
   - Repository URL: `https://github.com/AduAdrian/tsttt.git`
   - Repository Path: `/home/yourusername/repositories/tsttt`
   - Branch: `master`

### Step 2: Configure Environment Variables
**cPanel → Environment Variables**, add:
```bash
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://*:80
ConnectionStrings__DefaultConnection=Data Source=/home/yourusername/app_data/app.db
```

### Step 3: Verify .NET Support
- Contact hosting provider to confirm **ASP.NET Core 8.0** support
- Some hosts may need manual .NET runtime installation

1. **Move database file**:
   ```bash
   mkdir -p ~/app_data
   cp ~/public_html/app.db ~/app_data/ 2>/dev/null || true
   ```

2. **Set permissions**:
   ```bash
   find ~/public_html -type f -exec chmod 644 {} \;
   find ~/public_html -type d -exec chmod 755 {} \;
   ```

3. **Check .NET Runtime**:
   - Ensure .NET 8.0 runtime is installed on server
   - Contact hosting support if needed

## Option 3: FTP Upload Method
If Git deployment is not available:

1. Use `deploy-all.ps1` locally to create deployment files
2. Use FTP client (FileZilla, WinSCP) to upload:
   - Upload `cpanel-deploy/public_html/*` → `/public_html/`
   - Upload `cpanel-deploy/app_data/*` → `/app_data/`
3. Set permissions via cPanel File Manager

## Troubleshooting

### Common Issues:
1. **"System cannot deploy"** → Use manual methods above
2. **Database errors** → Check path in connection string
3. **Permission denied** → Set correct file permissions (644/755)
4. **Application won't start** → Verify .NET runtime installation

### Logs Location:
- Check `/logs/` directory for application logs
- cPanel Error Logs in cPanel → Error Logs section

## Support
If you need help with cPanel-specific setup:
1. Contact your hosting provider's support
2. Ask about ASP.NET Core 8.0 support
3. Request help with Git deployment setup