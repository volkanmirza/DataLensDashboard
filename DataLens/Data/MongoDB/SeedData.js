// MongoDB Seed Data Script
// Run this script after InitializeCollections.js
// This script populates the database with initial data

// Switch to datalens database
use('datalens');

// Helper function to generate ObjectId
function generateObjectId() {
  return new ObjectId();
}

// Helper function to hash password (SHA256 equivalent)
function hashPassword(password) {
  // Note: In production, use proper password hashing like bcrypt
  // This is a simplified version for demonstration
  return password; // In real implementation, this would be properly hashed
}

// Check if data already exists
const existingUsers = db.Users.countDocuments();
if (existingUsers > 0) {
  print('Data already exists. Skipping seed data insertion.');
} else {
  print('Inserting seed data...');

  // Create default users
  const adminUserId = generateObjectId();
  const designerUserId = generateObjectId();
  const viewerUserId = generateObjectId();

  const users = [
    {
      _id: adminUserId,
      Username: 'admin',
      Email: 'admin@datalens.com',
      PasswordHash: 'jGl25bVBBBW96Qi9Te4V37Fnqchz/Eu4qB9vKrRIqRg=', // SHA256 hash of 'admin123'
      FirstName: 'System',
      LastName: 'Administrator',
      Role: 'Admin',
      IsActive: true,
      CreatedAt: new Date(),
      UpdatedAt: null,
      LastLoginAt: null
    },
    {
      _id: designerUserId,
      Username: 'designer',
      Email: 'designer@datalens.com',
      PasswordHash: 'XohImNooBHFR0OVvjcYpJ3NgPQ1qq73WKhHvch0VQtg=', // SHA256 hash of 'designer123'
      FirstName: 'Dashboard',
      LastName: 'Designer',
      Role: 'Designer',
      IsActive: true,
      CreatedAt: new Date(),
      UpdatedAt: null,
      LastLoginAt: null
    },
    {
      _id: viewerUserId,
      Username: 'viewer',
      Email: 'viewer@datalens.com',
      PasswordHash: 'fEqNCco3Yq9h5ZUglD3CZJT4lBs+zPpaWkDWXamML9o=', // SHA256 hash of 'viewer123'
      FirstName: 'Report',
      LastName: 'Viewer',
      Role: 'Viewer',
      IsActive: true,
      CreatedAt: new Date(),
      UpdatedAt: null,
      LastLoginAt: null
    }
  ];

  // Insert users
  db.Users.insertMany(users);
  print('Users inserted successfully.');

  // Create default user groups
  const adminGroupId = generateObjectId();
  const designerGroupId = generateObjectId();
  const viewerGroupId = generateObjectId();

  const userGroups = [
    {
      _id: adminGroupId,
      GroupName: 'Administrators',
      Description: 'System administrators with full access',
      IsActive: true,
      CreatedAt: new Date(),
      UpdatedAt: null
    },
    {
      _id: designerGroupId,
      GroupName: 'Dashboard Designers',
      Description: 'Users who can create and edit dashboards',
      IsActive: true,
      CreatedAt: new Date(),
      UpdatedAt: null
    },
    {
      _id: viewerGroupId,
      GroupName: 'Report Viewers',
      Description: 'Users who can only view dashboards and reports',
      IsActive: true,
      CreatedAt: new Date(),
      UpdatedAt: null
    }
  ];

  // Insert user groups
  db.UserGroups.insertMany(userGroups);
  print('User groups inserted successfully.');

  // Create user group memberships
  const userGroupMembers = [
    {
      _id: generateObjectId(),
      UserId: adminUserId,
      GroupId: adminGroupId,
      IsActive: true,
      JoinedAt: new Date(),
      LeftAt: null
    },
    {
      _id: generateObjectId(),
      UserId: designerUserId,
      GroupId: designerGroupId,
      IsActive: true,
      JoinedAt: new Date(),
      LeftAt: null
    },
    {
      _id: generateObjectId(),
      UserId: viewerUserId,
      GroupId: viewerGroupId,
      IsActive: true,
      JoinedAt: new Date(),
      LeftAt: null
    }
  ];

  // Insert user group memberships
  db.UserGroupMembers.insertMany(userGroupMembers);
  print('User group memberships inserted successfully.');

  // Create sample dashboard
  const sampleDashboardId = generateObjectId();
  const sampleDashboard = {
    _id: sampleDashboardId,
    Title: 'Sample Sales Dashboard',
    Description: 'A sample dashboard showing sales metrics and KPIs',
    Category: 'Sales',
    Tags: ['sales', 'revenue', 'kpi', 'sample'],
    CreatedBy: designerUserId,
    IsPublic: true,
    IsActive: true,
    CreatedAt: new Date(),
    UpdatedAt: null,
    LastModifiedAt: new Date(),
    DashboardData: {
      layout: {
        rows: [
          {
            columns: [
              {
                width: 6,
                widgets: [
                  {
                    type: 'chart',
                    title: 'Monthly Sales',
                    config: {
                      chartType: 'line',
                      dataSource: 'sales_data'
                    }
                  }
                ]
              },
              {
                width: 6,
                widgets: [
                  {
                    type: 'metric',
                    title: 'Total Revenue',
                    config: {
                      value: '$125,000',
                      trend: '+12%'
                    }
                  }
                ]
              }
            ]
          }
        ]
      },
      theme: 'default',
      refreshInterval: 300
    }
  };

  // Insert sample dashboard
  db.Dashboards.insertOne(sampleDashboard);
  print('Sample dashboard inserted successfully.');

  // Create sample dashboard permissions
  const dashboardPermissions = [
    {
      _id: generateObjectId(),
      DashboardId: sampleDashboardId,
      UserId: adminUserId,
      GroupId: null,
      PermissionType: 'Delete',
      IsActive: true,
      GrantedAt: new Date(),
      RevokedAt: null,
      GrantedBy: adminUserId
    },
    {
      _id: generateObjectId(),
      DashboardId: sampleDashboardId,
      UserId: null,
      GroupId: designerGroupId,
      PermissionType: 'Edit',
      IsActive: true,
      GrantedAt: new Date(),
      RevokedAt: null,
      GrantedBy: adminUserId
    },
    {
      _id: generateObjectId(),
      DashboardId: sampleDashboardId,
      UserId: null,
      GroupId: viewerGroupId,
      PermissionType: 'View',
      IsActive: true,
      GrantedAt: new Date(),
      RevokedAt: null,
      GrantedBy: adminUserId
    }
  ];

  // Insert dashboard permissions
  db.DashboardPermissions.insertMany(dashboardPermissions);
  print('Dashboard permissions inserted successfully.');

  print('\n=== Seed Data Summary ===');
  print('Users created: 3 (admin, designer, viewer)');
  print('User groups created: 3 (Administrators, Dashboard Designers, Report Viewers)');
  print('Sample dashboard created: 1 (Sample Sales Dashboard)');
  print('Dashboard permissions created: 3');
  print('\n=== Default Login Credentials ===');
  print('Admin: admin / admin123');
  print('Designer: designer / designer123');
  print('Viewer: viewer / viewer123');
  print('\nSeed data insertion completed successfully!');
}

// Verify data insertion
print('\n=== Database Statistics ===');
print('Users count: ' + db.Users.countDocuments());
print('UserGroups count: ' + db.UserGroups.countDocuments());
print('UserGroupMembers count: ' + db.UserGroupMembers.countDocuments());
print('Dashboards count: ' + db.Dashboards.countDocuments());
print('DashboardPermissions count: ' + db.DashboardPermissions.countDocuments());