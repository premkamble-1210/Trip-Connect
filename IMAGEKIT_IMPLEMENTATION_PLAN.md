# ImageKit Image Upload Implementation Plan

## Overview
Implement ImageKit integration for image upload functionality with URL response. The implementation will follow the clean architecture pattern across multiple layers.

---

## Architecture Decision

**Yes, your intuition is correct!** The implementation should span **all three layers**:

```
API_LAYER (Controller)
    ↓
APPLICATION_LAYER (Service)
    ↓
INFRASTRUCTURE_LAYER (ImageKit Wrapper)
```

### Why This Approach?
- **INFRASTRUCTURE_LAYER**: Handles external service integration (ImageKit client, credentials, configuration)
- **APPLICATION_LAYER**: Contains business logic for image processing/validation
- **API_LAYER**: Exposes HTTP endpoints for clients

---

## Detailed Implementation Plan

### Phase 1: Infrastructure Layer Setup ✅ TO DO

#### 1.1 Install NuGet Package
```
Install-Package ImageKit
```
**Location**: INFRASTRUCTURE_LAYER.csproj

#### 1.2 Create ImageKit Service Interface
**File**: `INFRASTRUCTURE_LAYER/Services/IImageKitService.cs`

```csharp
public interface IImageKitService
{
    Task<ImageUploadResponse> UploadImageAsync(Stream imageStream, string fileName, string folder = "uploads");
    Task<bool> DeleteImageAsync(string fileId);
    Task<string> GetImageUrlAsync(string fileId, int? width = null, int? height = null);
}
```

#### 1.3 Create ImageKit Service Implementation
**File**: `INFRASTRUCTURE_LAYER/Services/ImageKitService.cs`

**Responsibilities**:
- Initialize ImageKit client with credentials
- Handle image upload to ImageKit
- Handle image deletion
- Generate optimized image URLs
- Error handling for ImageKit operations

**Key Methods**:
- `UploadImageAsync()` - Upload image and return file ID + URL
- `DeleteImageAsync()` - Delete image from ImageKit
- `GetImageUrlAsync()` - Generate optimized URLs

#### 1.4 Update Configuration
**File**: `API_LAYER/appsettings.json`

Add ImageKit credentials section:
```json
{
  "ImageKit": {
    "PublicKey": "your_public_key",
    "PrivateKey": "your_private_key",
    "UrlEndpoint": "https://ik.imagekit.io/your_id"
  }
}
```

#### 1.5 Update Dependency Injection
**File**: `INFRASTRUCTURE_LAYER/DependencyInjection.cs`

Register `IImageKitService` with its implementation

---

### Phase 2: Application Layer Setup ✅ TO DO

#### 2.1 Create DTOs (Data Transfer Objects)

**File**: `APPLICATION_LAYER/DTOs/ImageUploadRequestDto.cs`
```
Properties:
- IFormFile ImageFile
- string Folder (optional)
- string Description (optional)
```

**File**: `APPLICATION_LAYER/DTOs/ImageUploadResponseDto.cs`
```
Properties:
- string FileId
- string ImageUrl
- string PublicUrl
- DateTime UploadedAt
- bool Success
- string Message
```

#### 2.2 Create Image Service Interface
**File**: `APPLICATION_LAYER/Services/Interfaces/IImageService.cs`

```csharp
public interface IImageService
{
    Task<ImageUploadResponseDto> UploadProfileImageAsync(IFormFile image, int userId);
    Task<ImageUploadResponseDto> UploadTripImageAsync(IFormFile image, int tripId);
    Task<ImageUploadResponseDto> UploadExpenseImageAsync(IFormFile image, int expenseId);
    Task<bool> DeleteImageAsync(string fileId);
}
```

#### 2.3 Create Image Service Implementation
**File**: `APPLICATION_LAYER/Services/Implementations/ImageService.cs`

**Responsibilities**:
- Validate image file (size, format, dimensions)
- Organize uploads by folder structure
- Call Infrastructure ImageKitService
- Map responses to DTOs
- Handle business logic validations
- Logging and error handling

**Validation Rules**:
- File size: Max 5MB
- Allowed formats: jpg, jpeg, png, webp
- Min dimensions: 100x100px
- Max dimensions: 4000x4000px

---

### Phase 3: API Layer Setup ✅ TO DO

#### 3.1 Create Image Upload Controller
**File**: `API_LAYER/Controllers/ImageController.cs`

**Endpoints**:
```
POST /api/images/upload-profile
POST /api/images/upload-trip
POST /api/images/upload-expense
DELETE /api/images/{fileId}
```

**Responsibilities**:
- Accept multipart/form-data requests
- Call ApplicationLayer ImageService
- Return ImageUploadResponseDto with URL
- Handle request validation
- Return appropriate HTTP status codes

#### 3.2 Response Structure
```json
{
  "success": true,
  "message": "Image uploaded successfully",
  "data": {
    "fileId": "imagekit_file_id",
    "imageUrl": "https://ik.imagekit.io/...",
    "publicUrl": "https://ik.imagekit.io/...",
    "uploadedAt": "2026-04-30T12:00:00Z"
  }
}
```

---

## File Structure To Create

```
INFRASTRUCTURE_LAYER/
├── Services/
│   ├── IImageKitService.cs          (NEW)
│   └── ImageKitService.cs           (NEW)
├── DependencyInjection.cs           (MODIFY)
└── INFRASTRUCTURE_LAYER.csproj      (MODIFY - add NuGet package)

APPLICATION_LAYER/
├── DTOs/
│   ├── ImageUploadRequestDto.cs     (NEW)
│   └── ImageUploadResponseDto.cs    (NEW)
├── Services/
│   ├── Interfaces/
│   │   └── IImageService.cs         (NEW)
│   └── Implementations/
│       └── ImageService.cs          (NEW)
└── DependencyInjection.cs           (MODIFY)

API_LAYER/
├── Controllers/
│   └── ImageController.cs           (NEW)
└── appsettings.json                 (MODIFY - add credentials)
```

---

## Implementation Sequence

1. **Step 1**: Add NuGet package to INFRASTRUCTURE_LAYER.csproj
2. **Step 2**: Add ImageKit credentials to appsettings.json
3. **Step 3**: Create IImageKitService interface (Infrastructure)
4. **Step 4**: Create ImageKitService implementation (Infrastructure)
5. **Step 5**: Register in Infrastructure DependencyInjection.cs
6. **Step 6**: Create DTOs (Application Layer)
7. **Step 7**: Create IImageService interface (Application Layer)
8. **Step 8**: Create ImageService implementation (Application Layer)
9. **Step 9**: Register in Application DependencyInjection.cs
10. **Step 10**: Create ImageController (API Layer)
11. **Step 11**: Test endpoints with Postman/HTTP client

---

## Data Flow Diagram

```
Client (Frontend)
    ↓ (multipart/form-data with image file)
ImageController.UploadProfileImageAsync()
    ↓
IImageService (Business Logic Validation)
    ├─ Validate file size, format, dimensions
    ├─ Organize folder structure
    └─ Call IImageKitService
        ↓
IImageKitService (External Integration)
    ├─ Initialize ImageKit client
    ├─ Upload to ImageKit
    └─ Return FileId + URLs
    ↓
ImageService (Map to DTO)
    ↓
ImageController (Return Response)
    ↓
Client (Receives ImageUploadResponseDto with URL)
```

---

## Configuration Examples

### appsettings.json
```json
{
  "ImageKit": {
    "PublicKey": "your_imagekit_public_key",
    "PrivateKey": "your_imagekit_private_key",
    "UrlEndpoint": "https://ik.imagekit.io/your_imagekit_id"
  },
  "ImageUpload": {
    "MaxFileSizeInMB": 5,
    "AllowedFormats": ["jpg", "jpeg", "png", "webp"],
    "MinWidth": 100,
    "MinHeight": 100,
    "MaxWidth": 4000,
    "MaxHeight": 4000
  }
}
```

---

## Important Considerations

### Security
- ✅ Validate file type (magic numbers, not just extension)
- ✅ Validate file size on server-side
- ✅ Sanitize file names
- ✅ Use ImageKit's private key only on backend (never expose to client)
- ✅ Implement authentication on image endpoints

### Performance
- ✅ Use ImageKit's CDN for fast image delivery
- ✅ Generate multiple sizes for responsive images
- ✅ Use caching for frequently accessed images
- ✅ Implement async/await for non-blocking operations

### Error Handling
- ✅ Validate image before upload
- ✅ Handle ImageKit API failures gracefully
- ✅ Log all image operations
- ✅ Return meaningful error messages

---

## Next Steps After Planning

Once plan is approved:
1. Execute implementation in sequence (10 steps above)
2. Add Unit Tests for ImageService validation
3. Create Integration Tests for ImageController
4. Update Frontend to use image upload endpoints
5. Add image optimization transformations as needed

---

## Dependencies Required

```
ImageKit (NuGet Package)
Microsoft.AspNetCore.Http (for IFormFile)
System.Drawing.Common (for image validation if needed)
```

---

## Resources

- ImageKit Documentation: https://docs.imagekit.io/
- ImageKit .NET SDK: https://github.com/imagekit-developer/imagekit-dotnet
- Best Practices: Keep validation in Application Layer, external calls in Infrastructure Layer

