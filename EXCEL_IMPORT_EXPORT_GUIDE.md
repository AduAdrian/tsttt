# Excel/CSV Import/Export Features for Clients

## ?? Features Added

### Template Download
- **Excel Template**: Fully formatted template with validation rules and examples
- **Instructions**: Detailed guidance on data formats and validation rules
- **Examples**: Sample data showing correct formatting

### Data Export
- **Excel Export**: All client data exported in formatted Excel file with color coding
- **CSV Export**: Simple CSV format for compatibility with other systems
- **Automatic filename**: Files named with current date

### Bulk Import
- **Excel Import**: Process up to 1000 clients from .xlsx files
- **Data Validation**: Comprehensive validation for all fields
- **Duplicate Handling**: Existing clients are reactivated with new data
- **Error Reporting**: Detailed feedback on processing results

## ?? Template Structure

The Excel template includes:

### Columns:
1. **Nr. Inmatriculare** (Required)
   - Format: 2-3 letters + 2-3 digits + 3 letters (e.g., B123ABC)
   - Maximum 15 characters
   - Must be unique

2. **Numar Telefon** (Optional)
   - Romanian format: 07XXXXXXXX or +407XXXXXXXX  
   - 10 digits (without prefix) or 13 with +40
   - Used for SMS notifications

3. **Tip Valabilitate** (Required)
   - Values: Manual, 6 Luni, 1 An, 2 Ani
   - Case insensitive

4. **Data Expirare ITP** (Conditional)
   - For Manual type: DD/MM/YYYY or DD-MM-YYYY
   - For others: leave empty (calculated automatically)
   - Cannot be in the past

### Validation Rules:
- **Registration Number**: Romanian car plate format
- **Phone Number**: Normalized to Romanian mobile format
- **Expiry Date**: Future date validation
- **File Size**: Maximum 5MB
- **Row Limit**: Maximum 1000 records per import

## ?? Usage Instructions

### To Download Template:
1. Go to Clients page
2. Click **"Template Excel"** in the Import/Export section
3. Open the downloaded file and follow the examples

### To Import Data:
1. Fill the Excel template with your data
2. Click **"Import Excel"** button
3. Select your .xlsx file
4. Click **"Începe Import"**
5. Review the results message

### To Export Data:
1. Click **"Export Excel"** for formatted Excel with colors
2. Click **"Export CSV"** for simple comma-separated values
3. Files are automatically downloaded with timestamp

## ?? Important Notes

- **File Format**: Only .xlsx files accepted for import
- **Duplicates**: Inactive clients are reactivated; active clients are skipped
- **Validation**: Invalid data is reported but doesn't stop the process
- **Performance**: Large imports may take several minutes
- **Backup**: Consider backing up data before large imports

## ?? Error Handling

The system provides detailed feedback for:
- Invalid registration number formats
- Incorrect phone numbers
- Invalid dates
- Duplicate entries
- File format issues
- Size limit exceeded

Example error message:
```
Import finalizat: 150 clienti procesati cu succes
Avertismente: 5 randuri cu probleme. Primele: Rand 12: Format nr. inmatriculare invalid - ignorat; Rand 25: Data expirare invalida pentru tip Manual - ignorat
```

## ?? Status Indicators

In exported files, clients are color-coded:
- **Red**: Expired ITP (overdue)
- **Yellow**: Expiring soon (within 30 days)
- **White/Default**: Valid for more than 30 days

## ?? Mobile Support

The import/export interface is fully responsive and works on:
- Desktop computers
- Tablets
- Mobile phones

## ??? Technical Details

- **Processing Engine**: EPPlus for Excel handling
- **Validation**: Server-side with detailed feedback
- **Performance**: Optimized for bulk operations
- **Security**: File type and size validation
- **Database**: Transactional processing to ensure consistency

---

**Ready to use!** Navigate to `/Clients` to access all these features.