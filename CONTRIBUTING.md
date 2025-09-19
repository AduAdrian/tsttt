# Contribuire la Notificari-Progamari

Mul?umim c? vrei s? contribui la acest proiect! Aici sunt câteva ghiduri pentru a începe.

## Cum s? Contribui

### Raportare Bug-uri
- Folose?te GitHub Issues pentru a raporta bug-uri
- Include pa?i de reproducere
- Specific? versiunea ?i browser-ul folosit
- Adaug? screenshot-uri dac? este relevant

### Solicitare Func?ionalit??i Noi
- Deschide un Issue pentru discu?ie înainte de implementare
- Explic? cazul de utilizare
- Descrie comportamentul a?teptat

### Pull Requests
1. Fork repository-ul
2. Creeaz? un branch pentru feature-ul t?u: `git checkout -b feature-nume-feature`
3. Comite modific?rile: `git commit -m 'Adaug? func?ionalitate nou?'`
4. Push la branch: `git push origin feature-nume-feature`
5. Deschide un Pull Request

## Standarde de Cod

### C# / .NET
- Folose?te PascalCase pentru clase ?i metode
- Folose?te camelCase pentru variabile locale
- Adaug? comentarii XML pentru metode publice
- Respect? principiile SOLID

### Frontend
- Folose?te ES6+ pentru JavaScript
- Respect? conven?ia de naming pentru CSS classes
- Testeaz? cross-browser compatibility

### Baza de Date
- Folose?te EF Core Migrations pentru schimb?ri
- Adaug? seed data pentru teste
- Documenteaz? schema changes

## Testare
- Scrie unit tests pentru business logic
- Adaug? integration tests pentru API endpoints
- Testeaz? manual UI changes
- Asigur?-te c? toate testele trec: `dotnet test`

## Documenta?ie
- Actualizeaz? README.md pentru func?ionalit??i noi
- Adaug? documente tehnice în folderul `/docs`
- Comenteaz? cod complex
- Actualizeaz? API documentation

## Release Process
1. Actualizeaz? versiunea în `*.csproj`
2. Actualizeaz? CHANGELOG.md
3. Creeaz? release tag: `git tag v1.x.x`
4. Deploy pe staging pentru testare
5. Deploy pe produc?ie dup? aprobare

## Contact
Pentru întreb?ri despre contribu?ii, contacteaz?:
- Email: notificari-sms@misedainspectsrl.ro
- GitHub Issues pentru discu?ii tehnice