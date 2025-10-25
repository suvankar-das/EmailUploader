# EmailUploader Solution

This solution contains two projects:

- **EmailUploader.API** → ASP.NET Web API project
- **EmailUploader.UI** → ASP.NET MVC project

Both projects can be started together using Visual Studio’s _Multiple Startup Projects_ setting.

---

## 🚀 How to Run

1. Open the solution in **Visual Studio**
2. Go to:  
   **Solution Explorer → Right-click on Solution → Set Startup Projects → Select "Multiple startup projects"**
3. Set both projects (Web + API) to **Start**
4. Run the solution (**Start Without Debugging**)

---

## ✅ Test API

To confirm the API is running correctly, open your browser and visit:

```
http://localhost:5000/api/email/test
```

If everything is configured properly, you should see a success response like:

```json
{
  "Message": "API is working FINE!",
  "Timestamp": "Time_stamp"
}
```

---

## ⚙️ Database Configuration

Before running, make sure to update the **database connection string** in:

```
EmailUploader.API/Web.config
```

Locate this section:

```xml
<connectionStrings>
  <add name="DefaultConnection"
       connectionString="Data Source=YOUR_SERVER_NAME;Initial Catalog=YOUR_DATABASE;User ID=USERNAME;Password=PASSWORD;"
       providerName="System.Data.SqlClient" />
</connectionStrings>
```

Replace the placeholders (`YOUR_SERVER_NAME`, `YOUR_DATABASE`, etc.) with your actual database credentials.

---

## 🧩 Notes

- MVC Project URL: `http://localhost:5001`
- API Project URL: `http://localhost:5000`
- CORS is enabled in the API, so cross-origin requests from MVC will work.
- Make sure both projects run under different ports to avoid conflicts.

---
