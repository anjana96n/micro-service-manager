# Troubleshooting Guide

## Issue: Service not appearing after clicking Save

### What I fixed:

1. **Added explicit DataGrid refresh** - After adding a service, the grid now explicitly refreshes
2. **Added better error handling** - Any errors will now show a detailed message
3. **Set dialog Owner** - Dialogs are now properly modal and centered on parent
4. **Initialized Service property** - Prevents null reference issues
5. **Added startup logging** - Console shows when app starts successfully

### To test the fix:

1. **Close the currently running application completely**

2. **Rebuild the application** (recommended):
   ```powershell
   .\rebuild.ps1
   ```
   
   Or manually:
   ```powershell
   dotnet clean
   dotnet build
   ```

3. **Run the application**:
   ```powershell
   dotnet run
   ```

4. **Try adding a service**:
   - Click ➕ Add Service
   - Fill in all fields:
     - Service Name: `Test Service`
     - Project Path: `D:\Test` (or any path)
     - Run Command: `mvn spring-boot:run` (or any command)
   - Click Save

### What to look for:

✅ **Success**: 
- Service appears in the grid immediately
- Console shows: `[HH:MM:SS] Added service: Test Service`

❌ **If you see an error message**:
- Take a screenshot or copy the error text
- The error will show exactly what went wrong

❌ **If nothing happens**:
- Check the console output at the bottom
- Look for any error messages

### Common Issues:

#### 1. Dialog doesn't open
**Cause**: Application might be in an error state
**Fix**: 
- Close app completely (check Task Manager)
- Run `.\rebuild.ps1`
- Start fresh with `dotnet run`

#### 2. Service saves but doesn't appear
**Cause**: DataGrid not updating (should be fixed now)
**Fix**: Already implemented - grid now refreshes explicitly

#### 3. "Error initializing application" on startup
**Cause**: Issue with resources or data binding
**Fix**: Check the error message - it will tell you exactly what's wrong

#### 4. Application crashes silently
**Cause**: Unhandled exception
**Fix**: Run from command line to see errors:
```powershell
dotnet run
```
Any crashes will show error messages in the terminal.

### Debug Mode

To run in debug mode and see all output:

```powershell
dotnet run --configuration Debug
```

### Check Configuration File

Your services are saved at:
```
%APPDATA%\MicroserviceManager\services.json
```

To view it:
```powershell
notepad $env:APPDATA\MicroserviceManager\services.json
```

If this file is corrupted, you can delete it and start fresh:
```powershell
Remove-Item $env:APPDATA\MicroserviceManager\services.json
```

### Still Having Issues?

Run this command to get full diagnostic output:
```powershell
dotnet run --verbosity detailed
```

This will show exactly what's happening during execution.

