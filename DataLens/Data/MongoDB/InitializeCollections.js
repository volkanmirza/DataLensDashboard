// MongoDB Collection Initialization Script
// Run this script in MongoDB shell or MongoDB Compass

// Switch to datalens database
use('datalens');

// Create Users collection with validation schema
db.createCollection('Users', {
  validator: {
    $jsonSchema: {
      bsonType: 'object',
      required: ['Username', 'Email', 'PasswordHash', 'Role', 'IsActive', 'CreatedAt'],
      properties: {
        _id: {
          bsonType: 'objectId'
        },
        Username: {
          bsonType: 'string',
          minLength: 3,
          maxLength: 50,
          description: 'Username must be a string between 3-50 characters'
        },
        Email: {
          bsonType: 'string',
          pattern: '^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$',
          description: 'Email must be a valid email address'
        },
        PasswordHash: {
          bsonType: 'string',
          description: 'Password hash is required'
        },
        FirstName: {
          bsonType: ['string', 'null'],
          maxLength: 100
        },
        LastName: {
          bsonType: ['string', 'null'],
          maxLength: 100
        },
        Role: {
          bsonType: 'string',
          enum: ['Admin', 'Designer', 'Viewer'],
          description: 'Role must be one of Admin, Designer, or Viewer'
        },
        IsActive: {
          bsonType: 'bool',
          description: 'IsActive must be a boolean'
        },
        CreatedAt: {
          bsonType: 'date',
          description: 'CreatedAt must be a date'
        },
        UpdatedAt: {
          bsonType: ['date', 'null']
        },
        LastLoginAt: {
          bsonType: ['date', 'null']
        }
      }
    }
  }
});

// Create indexes for Users collection
db.Users.createIndex({ 'Username': 1 }, { unique: true });
db.Users.createIndex({ 'Email': 1 }, { unique: true });
db.Users.createIndex({ 'Role': 1 });
db.Users.createIndex({ 'IsActive': 1 });
db.Users.createIndex({ 'CreatedAt': 1 });

// Create UserGroups collection with validation schema
db.createCollection('UserGroups', {
  validator: {
    $jsonSchema: {
      bsonType: 'object',
      required: ['GroupName', 'IsActive', 'CreatedAt'],
      properties: {
        _id: {
          bsonType: 'objectId'
        },
        GroupName: {
          bsonType: 'string',
          minLength: 2,
          maxLength: 100,
          description: 'GroupName must be a string between 2-100 characters'
        },
        Description: {
          bsonType: ['string', 'null'],
          maxLength: 500
        },
        IsActive: {
          bsonType: 'bool',
          description: 'IsActive must be a boolean'
        },
        CreatedAt: {
          bsonType: 'date',
          description: 'CreatedAt must be a date'
        },
        UpdatedAt: {
          bsonType: ['date', 'null']
        }
      }
    }
  }
});

// Create indexes for UserGroups collection
db.UserGroups.createIndex({ 'GroupName': 1 }, { unique: true });
db.UserGroups.createIndex({ 'IsActive': 1 });
db.UserGroups.createIndex({ 'CreatedAt': 1 });

// Create UserGroupMembers collection with validation schema
db.createCollection('UserGroupMembers', {
  validator: {
    $jsonSchema: {
      bsonType: 'object',
      required: ['UserId', 'GroupId', 'IsActive', 'JoinedAt'],
      properties: {
        _id: {
          bsonType: 'objectId'
        },
        UserId: {
          bsonType: 'objectId',
          description: 'UserId must be a valid ObjectId'
        },
        GroupId: {
          bsonType: 'objectId',
          description: 'GroupId must be a valid ObjectId'
        },
        IsActive: {
          bsonType: 'bool',
          description: 'IsActive must be a boolean'
        },
        JoinedAt: {
          bsonType: 'date',
          description: 'JoinedAt must be a date'
        },
        LeftAt: {
          bsonType: ['date', 'null']
        }
      }
    }
  }
});

// Create indexes for UserGroupMembers collection
db.UserGroupMembers.createIndex({ 'UserId': 1, 'GroupId': 1 }, { unique: true });
db.UserGroupMembers.createIndex({ 'UserId': 1 });
db.UserGroupMembers.createIndex({ 'GroupId': 1 });
db.UserGroupMembers.createIndex({ 'IsActive': 1 });
db.UserGroupMembers.createIndex({ 'JoinedAt': 1 });

// Create Dashboards collection with validation schema
db.createCollection('Dashboards', {
  validator: {
    $jsonSchema: {
      bsonType: 'object',
      required: ['Title', 'CreatedBy', 'IsActive', 'CreatedAt'],
      properties: {
        _id: {
          bsonType: 'objectId'
        },
        Title: {
          bsonType: 'string',
          minLength: 1,
          maxLength: 200,
          description: 'Title must be a string between 1-200 characters'
        },
        Description: {
          bsonType: ['string', 'null'],
          maxLength: 1000
        },
        Category: {
          bsonType: ['string', 'null'],
          maxLength: 100
        },
        Tags: {
          bsonType: ['array', 'null'],
          items: {
            bsonType: 'string'
          }
        },
        CreatedBy: {
          bsonType: 'objectId',
          description: 'CreatedBy must be a valid ObjectId'
        },
        IsPublic: {
          bsonType: 'bool'
        },
        IsActive: {
          bsonType: 'bool',
          description: 'IsActive must be a boolean'
        },
        CreatedAt: {
          bsonType: 'date',
          description: 'CreatedAt must be a date'
        },
        UpdatedAt: {
          bsonType: ['date', 'null']
        },
        LastModifiedAt: {
          bsonType: ['date', 'null']
        },
        DashboardData: {
          bsonType: ['object', 'null'],
          description: 'Dashboard configuration and layout data'
        }
      }
    }
  }
});

// Create indexes for Dashboards collection
db.Dashboards.createIndex({ 'Title': 1 });
db.Dashboards.createIndex({ 'CreatedBy': 1 });
db.Dashboards.createIndex({ 'Category': 1 });
db.Dashboards.createIndex({ 'Tags': 1 });
db.Dashboards.createIndex({ 'IsPublic': 1 });
db.Dashboards.createIndex({ 'IsActive': 1 });
db.Dashboards.createIndex({ 'CreatedAt': 1 });
db.Dashboards.createIndex({ 'LastModifiedAt': 1 });

// Create DashboardPermissions collection with validation schema
db.createCollection('DashboardPermissions', {
  validator: {
    $jsonSchema: {
      bsonType: 'object',
      required: ['DashboardId', 'PermissionType', 'IsActive', 'GrantedAt'],
      properties: {
        _id: {
          bsonType: 'objectId'
        },
        DashboardId: {
          bsonType: 'objectId',
          description: 'DashboardId must be a valid ObjectId'
        },
        UserId: {
          bsonType: ['objectId', 'null'],
          description: 'UserId for user-specific permissions'
        },
        GroupId: {
          bsonType: ['objectId', 'null'],
          description: 'GroupId for group-specific permissions'
        },
        PermissionType: {
          bsonType: 'string',
          enum: ['View', 'Edit', 'Delete', 'Share'],
          description: 'PermissionType must be one of View, Edit, Delete, or Share'
        },
        IsActive: {
          bsonType: 'bool',
          description: 'IsActive must be a boolean'
        },
        GrantedAt: {
          bsonType: 'date',
          description: 'GrantedAt must be a date'
        },
        RevokedAt: {
          bsonType: ['date', 'null']
        },
        GrantedBy: {
          bsonType: ['objectId', 'null'],
          description: 'User who granted this permission'
        }
      }
    }
  }
});

// Create indexes for DashboardPermissions collection
db.DashboardPermissions.createIndex({ 'DashboardId': 1 });
db.DashboardPermissions.createIndex({ 'UserId': 1 });
db.DashboardPermissions.createIndex({ 'GroupId': 1 });
db.DashboardPermissions.createIndex({ 'DashboardId': 1, 'UserId': 1, 'PermissionType': 1 });
db.DashboardPermissions.createIndex({ 'DashboardId': 1, 'GroupId': 1, 'PermissionType': 1 });
db.DashboardPermissions.createIndex({ 'IsActive': 1 });
db.DashboardPermissions.createIndex({ 'GrantedAt': 1 });

print('MongoDB collections and indexes created successfully!');
print('Collections created: Users, UserGroups, UserGroupMembers, Dashboards, DashboardPermissions');
print('All collections have validation schemas and appropriate indexes.');