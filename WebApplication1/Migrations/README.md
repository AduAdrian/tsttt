# Entity Framework Migrations

Acest director con?ine migra?iile Entity Framework Core pentru aplica?ia WebApplication1.

## Comenzi utile pentru migra?ii:

### Crearea unei noi migra?ii
```bash
dotnet ef migrations add [NumeMigra?ie]
```

### Aplicarea migra?iilor în baza de date
```bash
dotnet ef database update
```

### Revenirea la o migra?ie anterioar?
```bash
dotnet ef database update [NumeMigra?iei]
```

### ?tergerea bazei de date
```bash
dotnet ef database drop
```

### ?tergerea ultimei migra?ii (care nu a fost aplicat?)
```bash
dotnet ef migrations remove
```

## Structura bazei de date

### Tabele principale:
- **AspNetUsers** - Utilizatori (Identity)
- **AspNetRoles** - Roluri (Identity) 
- **AspNetUserRoles** - Rela?ia Many-to-Many între utilizatori ?i roluri
- **Appointments** - Program?ri (tabela principal? a aplica?iei)

### Tabela Appointments:
- `Id` (int) - Cheia primar?
- `Title` (string, max 200) - Titlul program?rii
- `Description` (string, max 1000) - Descrierea detaliat?
- `AppointmentDate` (datetime) - Data ?i ora program?rii
- `DurationMinutes` (int) - Durata în minute
- `ClientName` (string, max 100) - Numele clientului
- `ClientPhone` (string, max 15) - Telefonul clientului
- `ClientEmail` (string, max 100) - Email-ul clientului  
- `Location` (string, max 200) - Loca?ia program?rii
- `Status` (enum) - Statusul program?rii
- `Notes` (string, max 500) - Note suplimentare
- `CreatedAt` (datetime) - Data cre?rii
- `UpdatedAt` (datetime?) - Data ultimei modific?ri
- `CreatedByUserId` (string?) - ID-ul utilizatorului care a creat programarea

### Indexe create:
- Index pe `AppointmentDate` pentru c?ut?ri rapide dup? dat?
- Index pe `ClientName` pentru c?ut?ri dup? nume client
- Index pe `Status` pentru filtr?ri dup? status
- Index pe `CreatedByUserId` pentru rela?ia cu utilizatorii

## Configurare automat?

Aplica?ia este configurat? s? aplice automat migra?iile la pornire prin `context.Database.Migrate()` în Program.cs.