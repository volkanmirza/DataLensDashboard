// MongoDB Collection Initialization Script
// Run this script in MongoDB shell or MongoDB Compass

// Switch to datalens database
use('datalens');

// Create Users collection with validation schema
db.createCollection('Users', {
  validator: {
    $jsonSchema: {
      bsonType: 'object',
      required: ['UserName', 'NormalizedUserName', 'Email', 'NormalizedEmail', 'PasswordHash', 'Role', 'IsActive', 'CreatedDate'],
      properties: {
        _id: {
          bsonType: 'objectId'
        },
        UserName: {
          bsonType: 'string',
          minLength: 3,
          maxLength: 50,
          description: 'UserName must be a string between 3-50 characters'
        },
        NormalizedUserName: {
          bsonType: 'string',
          description: 'Normalized username for case-insensitive searches'
        },
        Email: {
          bsonType: 'string',
          pattern: '^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$',
          description: 'Email must be a valid email address'
        },
        NormalizedEmail: {
          bsonType: 'string',
          description: 'Normalized email for case-insensitive searches'
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
        CreatedDate: {
          bsonType: 'date',
          description: 'CreatedDate must be a date'
        },
        UpdatedDate: {
          bsonType: ['date', 'null']
        },
        LastLoginDate: {
          bsonType: ['date', 'null']
        }
      }
    }
  }
});

// Create indexes for Users collection
db.Users.createIndex({ 'UserName': 1 }, { unique: true });
db.Users.createIndex({ 'NormalizedUserName': 1 }, { unique: true });
db.Users.createIndex({ 'Email': 1 }, { unique: true });
db.Users.createIndex({ 'NormalizedEmail': 1 }, { unique: true });
db.Users.createIndex({ 'Role': 1 });
db.Users.createIndex({ 'IsActive': 1 });
db.Users.createIndex({ 'CreatedDate': 1 });

// Create UserGroups collection with validation schema
db.createCollection('UserGroups', {
  validator: {
    $jsonSchema: {
      bsonType: 'object',
      required: ['GroupName', 'IsActive', 'CreatedDate'],
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
        CreatedDate: {
          bsonType: 'date',
          description: 'CreatedDate must be a date'
        },
        UpdatedDate: {
          bsonType: ['date', 'null']
        }
      }
    }
  }
});

// Create indexes for UserGroups collection
db.UserGroups.createIndex({ 'GroupName': 1 }, { unique: true });
db.UserGroups.createIndex({ 'IsActive': 1 });
db.UserGroups.createIndex({ 'CreatedDate': 1 });

// Create UserGroupMembers collection with validation schema
db.createCollection('UserGroupMembers', {
  validator: {
    $jsonSchema: {
      bsonType: 'object',
      required: ['UserId', 'GroupId', 'IsActive', 'JoinedDate'],
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
        JoinedDate: {
          bsonType: 'date',
          description: 'JoinedDate must be a date'
        },
        LeftDate: {
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
db.UserGroupMembers.createIndex({ 'JoinedDate': 1 });

// Create Dashboards collection with validation schema
db.createCollection('Dashboards', {
  validator: {
    $jsonSchema: {
      bsonType: 'object',
      required: ['Title', 'CreatedBy', 'IsActive', 'CreatedDate'],
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
        CreatedDate: {
          bsonType: 'date',
          description: 'CreatedDate must be a date'
        },
        UpdatedDate: {
          bsonType: ['date', 'null']
        },
        LastModifiedDate: {
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
db.Dashboards.createIndex({ 'CreatedDate': 1 });
db.Dashboards.createIndex({ 'LastModifiedDate': 1 });

// Create DashboardPermissions collection with validation schema
db.createCollection('DashboardPermissions', {
  validator: {
    $jsonSchema: {
      bsonType: 'object',
      required: ['DashboardId', 'PermissionType', 'IsActive', 'GrantedDate'],
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
        GrantedDate: {
          bsonType: 'date',
          description: 'GrantedDate must be a date'
        },
        RevokedDate: {
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
db.DashboardPermissions.createIndex({ 'GrantedDate': 1 });

print('MongoDB collections and indexes created successfully!');
print('Collections created: Users, UserGroups, UserGroupMembers, Dashboards, DashboardPermissions');
print('All collections have validation schemas and appropriate indexes.');