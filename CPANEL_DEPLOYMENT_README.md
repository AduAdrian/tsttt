# Manual Deployment Instructions for cPanel
# Use this if automatic Git deployment doesn't work

## Option 1: Use the PowerShell Scripts (Recommended)
1. Run locally: `.\deploy-all.ps1`
2. Compress the `cpanel-deploy` folder to ZIP
3. Upload ZIP to cPanel File Manager
4. Extract and follow instructions

## Option 2: Direct Git Deployment Setup
If cPanel supports Git deployment:

### Step 1: Enable Git Deployment in cPanel
1. Go to cPanel → Git™ Version Control
2. Create repository or connect existing one
3. Set Repository Path: `/home/yourusername/repositories/tsttt`
4. Set Deployment Path: `/home/yourusername/public_html`

### Step 2: Configure Environment Variables
In cPanel → Environment Variables, add:
```
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://*:80
ConnectionStrings__DefaultConnection=Data Source=/home/yourusername/app_data/app.db
```

### Step 3: Deploy
1. Click "Update from Remote" in cPanel Git interface
2. Or use "Deploy HEAD Commit" button

### Step 4: Manual Steps After Git Deploy
After each deployment, you may need to:

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