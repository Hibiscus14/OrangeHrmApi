# OrangeHRM Web API 

## Стек
- **ASP.NET Core 8 Web API**
- **Selenium WebDriver + Chrome** (headless)
- **Entity Framework Core + SQLite**
- **Serilog** для логирования

## Эндпоинты
- `POST /api/orangehrm/employees` - добавить сотрудника + Job
- `POST /api/orangehrm/claims` - создать претензию

## Ключевые особенности

### Selenium автоматизация
- Page Object Model (LoginPage, EmployeePage, JobPage, AssignClaimPage)
- Валидация всех выпадающих списков перед выбором
- Автогенерация EmployeeId (7 символов)
- Извлечение ReferenceId со страницы подтверждения

### База данных
- SQLite с таблицей employees
- Двухрежимная работа: по employeeId или по имени
- Проверка дубликатов (конфигурируемо)

### Обработка ошибок
- HTTP 400 - валидация, "Job title 'X' not found"
- HTTP 409 - дубликаты сотрудников  
- HTTP 500 - внутренние ошибки
- Детальное логирование всех операций

### Конфигурация
```json
{
  "OrangeHRM": {
    "HeadlessMode": true,
    "UseSqlite": true,
    "TimeoutSeconds": 30,
    "AllowDuplicateEmployees": false
  }
}
```
