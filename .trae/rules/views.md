# DataLens Dashboard - Views ve UI Kuralları

## Razor Views Geliştirme Kuralları

### 1. Layout Yapısı

#### _Layout.cshtml (Ana Layout)
```html
<!DOCTYPE html>
<html lang="tr">
<head>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <title>@ViewData["Title"] - DataLens Dashboard</title>
    
    <!-- AdminLTE CSS -->
    <link rel="stylesheet" href="~/lib/admin-lte/css/adminlte.min.css">
    <link rel="stylesheet" href="~/lib/fontawesome/css/all.min.css">
    
    @await RenderSectionAsync("Styles", required: false)
</head>
<body class="hold-transition sidebar-mini layout-fixed">
    <div class="wrapper">
        <!-- Header -->
        @await Html.PartialAsync("_Header")
        
        <!-- Sidebar -->
        @await Html.PartialAsync("_Sidebar")
        
        <!-- Content -->
        <div class="content-wrapper">
            @RenderBody()
        </div>
        
        <!-- Footer -->
        @await Html.PartialAsync("_Footer")
    </div>
    
    <!-- AdminLTE JS -->
    <script src="~/lib/jquery/jquery.min.js"></script>
    <script src="~/lib/bootstrap/js/bootstrap.bundle.min.js"></script>
    <script src="~/lib/admin-lte/js/adminlte.min.js"></script>
    
    @await RenderSectionAsync("Scripts", required: false)
</body>
</html>
```

#### _AdminLayout.cshtml (Admin Area Layout)
```html
@{
    Layout = "~/Views/Shared/_Layout.cshtml";
}

<div class="content-header">
    <div class="container-fluid">
        <div class="row mb-2">
            <div class="col-sm-6">
                <h1 class="m-0">@ViewData["Title"]</h1>
            </div>
            <div class="col-sm-6">
                <ol class="breadcrumb float-sm-right">
                    @await Html.PartialAsync("_Breadcrumb")
                </ol>
            </div>
        </div>
    </div>
</div>

<section class="content">
    <div class="container-fluid">
        @RenderBody()
    </div>
</section>
```

### 2. Partial Views

#### _Header.cshtml
```html
<nav class="main-header navbar navbar-expand navbar-white navbar-light">
    <!-- Left navbar links -->
    <ul class="navbar-nav">
        <li class="nav-item">
            <a class="nav-link" data-widget="pushmenu" href="#" role="button">
                <i class="fas fa-bars"></i>
            </a>
        </li>
    </ul>
    
    <!-- Right navbar links -->
    <ul class="navbar-nav ml-auto">
        <!-- Notifications -->
        @await Html.PartialAsync("_NotificationDropdown")
        
        <!-- User Menu -->
        @await Html.PartialAsync("_UserMenu")
    </ul>
</nav>
```

#### _Sidebar.cshtml
```html
<aside class="main-sidebar sidebar-dark-primary elevation-4">
    <!-- Brand Logo -->
    <a href="@Url.Action("Index", "Home")" class="brand-link">
        <img src="~/images/logo.png" alt="DataLens" class="brand-image img-circle elevation-3">
        <span class="brand-text font-weight-light">DataLens</span>
    </a>
    
    <!-- Sidebar -->
    <div class="sidebar">
        <!-- User Panel -->
        @await Html.PartialAsync("_UserPanel")
        
        <!-- Sidebar Menu -->
        @await Html.PartialAsync("_SidebarMenu")
    </div>
</aside>
```

### 3. Area Views Yapısı

#### Admin Area Views

##### Users/Index.cshtml
```html
@model IEnumerable<DataLens.Models.User>
@{
    ViewData["Title"] = "Kullanıcı Yönetimi";
    Layout = "~/Areas/Admin/Views/Shared/_AdminLayout.cshtml";
}

<div class="row">
    <div class="col-12">
        <div class="card">
            <div class="card-header">
                <h3 class="card-title">Kullanıcılar</h3>
                <div class="card-tools">
                    <a href="@Url.Action("Create")" class="btn btn-primary btn-sm">
                        <i class="fas fa-plus"></i> Yeni Kullanıcı
                    </a>
                </div>
            </div>
            <div class="card-body">
                <table id="usersTable" class="table table-bordered table-striped">
                    <thead>
                        <tr>
                            <th>Kullanıcı Adı</th>
                            <th>E-posta</th>
                            <th>Rol</th>
                            <th>Durum</th>
                            <th>Oluşturma Tarihi</th>
                            <th>İşlemler</th>
                        </tr>
                    </thead>
                    <tbody>
                        @foreach (var user in Model)
                        {
                            <tr>
                                <td>@user.Username</td>
                                <td>@user.Email</td>
                                <td>
                                    <span class="badge badge-@(GetRoleBadgeClass(user.Role))">@user.Role</span>
                                </td>
                                <td>
                                    <span class="badge badge-@(user.IsActive ? "success" : "danger")">
                                        @(user.IsActive ? "Aktif" : "Pasif")
                                    </span>
                                </td>
                                <td>@user.CreatedDate.ToString("dd.MM.yyyy")</td>
                                <td>
                                    <div class="btn-group">
                                        <a href="@Url.Action("Details", new { id = user.Id })" class="btn btn-info btn-sm">
                                            <i class="fas fa-eye"></i>
                                        </a>
                                        <a href="@Url.Action("Edit", new { id = user.Id })" class="btn btn-warning btn-sm">
                                            <i class="fas fa-edit"></i>
                                        </a>
                                        <a href="@Url.Action("Delete", new { id = user.Id })" class="btn btn-danger btn-sm">
                                            <i class="fas fa-trash"></i>
                                        </a>
                                    </div>
                                </td>
                            </tr>
                        }
                    </tbody>
                </table>
            </div>
        </div>
    </div>
</div>

@section Scripts {
    <script>
        $(document).ready(function() {
            $('#usersTable').DataTable({
                "responsive": true,
                "lengthChange": false,
                "autoWidth": false,
                "language": {
                    "url": "//cdn.datatables.net/plug-ins/1.10.24/i18n/Turkish.json"
                }
            });
        });
    </script>
}
```

##### Users/Create.cshtml
```html
@model DataLens.Models.User
@{
    ViewData["Title"] = "Yeni Kullanıcı";
    Layout = "~/Areas/Admin/Views/Shared/_AdminLayout.cshtml";
}

<div class="row">
    <div class="col-md-6">
        <div class="card card-primary">
            <div class="card-header">
                <h3 class="card-title">Kullanıcı Bilgileri</h3>
            </div>
            <form asp-action="Create" method="post">
                <div class="card-body">
                    <div class="form-group">
                        <label asp-for="Username" class="control-label"></label>
                        <input asp-for="Username" class="form-control" />
                        <span asp-validation-for="Username" class="text-danger"></span>
                    </div>
                    
                    <div class="form-group">
                        <label asp-for="Email" class="control-label"></label>
                        <input asp-for="Email" class="form-control" type="email" />
                        <span asp-validation-for="Email" class="text-danger"></span>
                    </div>
                    
                    <div class="form-group">
                        <label for="Password" class="control-label">Şifre</label>
                        <input name="Password" type="password" class="form-control" required />
                    </div>
                    
                    <div class="form-group">
                        <label asp-for="Role" class="control-label"></label>
                        <select asp-for="Role" class="form-control">
                            <option value="Viewer">Viewer</option>
                            <option value="Designer">Designer</option>
                            <option value="Admin">Admin</option>
                        </select>
                        <span asp-validation-for="Role" class="text-danger"></span>
                    </div>
                    
                    <div class="form-group">
                        <label asp-for="FirstName" class="control-label"></label>
                        <input asp-for="FirstName" class="form-control" />
                        <span asp-validation-for="FirstName" class="text-danger"></span>
                    </div>
                    
                    <div class="form-group">
                        <label asp-for="LastName" class="control-label"></label>
                        <input asp-for="LastName" class="form-control" />
                        <span asp-validation-for="LastName" class="text-danger"></span>
                    </div>
                </div>
                
                <div class="card-footer">
                    <button type="submit" class="btn btn-primary">Kaydet</button>
                    <a href="@Url.Action("Index")" class="btn btn-secondary">İptal</a>
                </div>
            </form>
        </div>
    </div>
</div>

@section Scripts {
    @{await Html.RenderPartialAsync("_ValidationScriptsPartial");}
}
```

#### Dashboard Area Views

##### Dashboards/Index.cshtml
```html
@model IEnumerable<DataLens.Models.Dashboard>
@{
    ViewData["Title"] = "Dashboard Yönetimi";
    Layout = "~/Areas/Dashboard/Views/Shared/_DashboardLayout.cshtml";
}

<div class="row">
    @foreach (var dashboard in Model)
    {
        <div class="col-lg-3 col-6">
            <div class="small-box bg-info">
                <div class="inner">
                    <h3>@dashboard.ViewCount</h3>
                    <p>@dashboard.Title</p>
                </div>
                <div class="icon">
                    <i class="fas fa-chart-bar"></i>
                </div>
                <a href="@Url.Action("View", new { id = dashboard.Id })" class="small-box-footer">
                    Görüntüle <i class="fas fa-arrow-circle-right"></i>
                </a>
            </div>
        </div>
    }
</div>

<div class="row">
    <div class="col-12">
        <div class="card">
            <div class="card-header">
                <h3 class="card-title">Dashboard Listesi</h3>
                <div class="card-tools">
                    @if (User.IsInRole("Admin") || User.IsInRole("Designer"))
                    {
                        <a href="@Url.Action("Create")" class="btn btn-primary btn-sm">
                            <i class="fas fa-plus"></i> Yeni Dashboard
                        </a>
                    }
                </div>
            </div>
            <div class="card-body">
                <div class="row">
                    @foreach (var dashboard in Model)
                    {
                        <div class="col-md-4">
                            <div class="card card-widget widget-user">
                                <div class="widget-user-header bg-info">
                                    <h3 class="widget-user-username">@dashboard.Title</h3>
                                    <h5 class="widget-user-desc">@dashboard.Category</h5>
                                </div>
                                <div class="widget-user-image">
                                    <img class="img-circle elevation-2" src="~/images/dashboard-icon.png" alt="Dashboard">
                                </div>
                                <div class="card-footer">
                                    <div class="row">
                                        <div class="col-sm-4 border-right">
                                            <div class="description-block">
                                                <h5 class="description-header">@dashboard.ViewCount</h5>
                                                <span class="description-text">Görüntülenme</span>
                                            </div>
                                        </div>
                                        <div class="col-sm-4 border-right">
                                            <div class="description-block">
                                                <h5 class="description-header">@dashboard.CreatedDate.ToString("dd.MM.yyyy")</h5>
                                                <span class="description-text">Oluşturulma</span>
                                            </div>
                                        </div>
                                        <div class="col-sm-4">
                                            <div class="description-block">
                                                <h5 class="description-header">@(dashboard.IsPublic ? "Genel" : "Özel")</h5>
                                                <span class="description-text">Durum</span>
                                            </div>
                                        </div>
                                    </div>
                                    <div class="row mt-2">
                                        <div class="col-12">
                                            <a href="@Url.Action("View", new { id = dashboard.Id })" class="btn btn-primary btn-sm">
                                                <i class="fas fa-eye"></i> Görüntüle
                                            </a>
                                            @if (User.IsInRole("Admin") || User.IsInRole("Designer"))
                                            {
                                                <a href="@Url.Action("Edit", new { id = dashboard.Id })" class="btn btn-warning btn-sm">
                                                    <i class="fas fa-edit"></i> Düzenle
                                                </a>
                                                <a href="@Url.Action("Clone", new { id = dashboard.Id })" class="btn btn-info btn-sm">
                                                    <i class="fas fa-copy"></i> Kopyala
                                                </a>
                                            }
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    }
                </div>
            </div>
        </div>
    </div>
</div>
```

### 4. Form Validation

#### Client-Side Validation
```html
@section Scripts {
    <script src="~/lib/jquery-validation/jquery.validate.min.js"></script>
    <script src="~/lib/jquery-validation-unobtrusive/jquery.validate.unobtrusive.min.js"></script>
    
    <script>
        $(document).ready(function() {
            // Custom validation rules
            $.validator.addMethod("strongpassword", function(value, element) {
                return this.optional(element) || /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$/.test(value);
            }, "Şifre en az 8 karakter olmalı ve büyük harf, küçük harf, rakam ve özel karakter içermelidir.");
            
            $("#passwordField").rules("add", {
                strongpassword: true
            });
        });
    </script>
}
```

### 5. AJAX Operations

#### AJAX Form Submission
```javascript
function submitFormAjax(formId, successCallback) {
    $(formId).on('submit', function(e) {
        e.preventDefault();
        
        var form = $(this);
        var url = form.attr('action');
        var data = form.serialize();
        
        $.ajax({
            url: url,
            type: 'POST',
            data: data,
            beforeSend: function() {
                showLoading();
            },
            success: function(response) {
                hideLoading();
                if (response.success) {
                    showToast('success', response.message);
                    if (successCallback) successCallback(response);
                } else {
                    showToast('error', response.message);
                }
            },
            error: function() {
                hideLoading();
                showToast('error', 'Bir hata oluştu.');
            }
        });
    });
}
```

### 6. Toast Notifications

#### Toast Helper
```javascript
function showToast(type, message) {
    $(document).Toasts('create', {
        class: 'bg-' + type,
        title: getToastTitle(type),
        body: message,
        autohide: true,
        delay: 5000
    });
}

function getToastTitle(type) {
    switch(type) {
        case 'success': return 'Başarılı';
        case 'error': return 'Hata';
        case 'warning': return 'Uyarı';
        case 'info': return 'Bilgi';
        default: return 'Bildirim';
    }
}
```

### 7. DataTables Configuration

#### Turkish DataTables
```javascript
function initializeDataTable(tableId, options = {}) {
    var defaultOptions = {
        "responsive": true,
        "lengthChange": false,
        "autoWidth": false,
        "language": {
            "url": "//cdn.datatables.net/plug-ins/1.10.24/i18n/Turkish.json"
        },
        "buttons": ["copy", "csv", "excel", "pdf", "print", "colvis"]
    };
    
    var finalOptions = $.extend({}, defaultOptions, options);
    
    $(tableId).DataTable(finalOptions).buttons().container().appendTo(tableId + '_wrapper .col-md-6:eq(0)');
}
```

### 8. Modal Operations

#### Dynamic Modal
```javascript
function showModal(title, content, size = 'modal-lg') {
    var modal = `
        <div class="modal fade" id="dynamicModal" tabindex="-1">
            <div class="modal-dialog ${size}">
                <div class="modal-content">
                    <div class="modal-header">
                        <h4 class="modal-title">${title}</h4>
                        <button type="button" class="close" data-dismiss="modal">
                            <span>&times;</span>
                        </button>
                    </div>
                    <div class="modal-body">
                        ${content}
                    </div>
                </div>
            </div>
        </div>
    `;
    
    $('body').append(modal);
    $('#dynamicModal').modal('show');
    
    $('#dynamicModal').on('hidden.bs.modal', function() {
        $(this).remove();
    });
}
```

### 9. Loading States

#### Loading Overlay
```javascript
function showLoading() {
    if ($('#loadingOverlay').length === 0) {
        $('body').append(`
            <div id="loadingOverlay" class="overlay">
                <i class="fas fa-2x fa-sync-alt fa-spin"></i>
            </div>
        `);
    }
    $('#loadingOverlay').show();
}

function hideLoading() {
    $('#loadingOverlay').hide();
}
```

### 10. View Conventions

#### Naming Conventions
- PascalCase for view names
- Descriptive folder structure
- Consistent partial view naming (_PartialName)
- Area-specific layouts

#### File Organization
```
Views/
├── Shared/
│   ├── _Layout.cshtml
│   ├── _Header.cshtml
│   ├── _Sidebar.cshtml
│   ├── _Footer.cshtml
│   └── Error.cshtml
├── Home/
│   ├── Index.cshtml
│   └── Privacy.cshtml
└── Account/
    ├── Login.cshtml
    └── Register.cshtml

Areas/
├── Admin/
│   └── Views/
│       ├── Shared/
│       │   └── _AdminLayout.cshtml
│       ├── Users/
│       └── UserGroups/
├── Dashboard/
│   └── Views/
└── Profile/
    └── Views/
```

### 11. Responsive Design

#### Bootstrap Grid Usage
```html
<div class="row">
    <div class="col-lg-3 col-md-6 col-sm-12">
        <!-- Content for large screens: 3 columns, medium: 2 columns, small: 1 column -->
    </div>
</div>
```

### 12. Security in Views

#### XSS Prevention
```html
<!-- Safe: Razor automatically encodes -->
<p>@Model.UserInput</p>

<!-- Unsafe: Raw HTML -->
<p>@Html.Raw(Model.TrustedHtml)</p>

<!-- Safe: Explicit encoding -->
<p>@Html.Encode(Model.UserInput)</p>
```

### 13. SEO and Accessibility

#### Meta Tags
```html
<head>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <meta name="description" content="@ViewData["Description"]">
    <meta name="keywords" content="@ViewData["Keywords"]">
    <title>@ViewData["Title"] - DataLens Dashboard</title>
</head>
```

#### Accessibility
```html
<!-- Proper labels -->
<label for="username">Kullanıcı Adı</label>
<input id="username" name="username" type="text" aria-required="true">

<!-- Alt text for images -->
<img src="chart.png" alt="Satış grafiği">

<!-- ARIA attributes -->
<button aria-label="Menüyü aç" data-widget="pushmenu">
    <i class="fas fa-bars"></i>
</button>
```

### 14. Performance Optimization

#### Bundle and Minification
```html
@section Styles {
    <environment include="Development">
        <link rel="stylesheet" href="~/css/custom.css" />
    </environment>
    <environment exclude="Development">
        <link rel="stylesheet" href="~/css/custom.min.css" asp-append-version="true" />
    </environment>
}
```

### 15. Internationalization

#### Resource Files
```html
@using Microsoft.Extensions.Localization
@inject IStringLocalizer<SharedResource> Localizer

<h1>@Localizer["Welcome"]</h1>
<p>@Localizer["Description"]</p>
```