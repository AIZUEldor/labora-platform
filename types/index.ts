// ==================== AUTH ====================
export interface LoginRequest {
  phoneNumber: string;
  password: string;
}

export interface RegisterRequest {
  firstName: string;
  lastName: string;
  age: number;
  phoneNumber: string;
  password: string;
  role: UserRole;
}

export interface AuthResponse {
  token: string;
  role: string;
  userId: string;
  firstName: string;
  lastName: string;
}

// ==================== LOGIN OTP ====================
// Start reuses LoginRequest above; Resend/Verify/Complete reuse
// StartOtpResponse/ResendOtpResponse/VerifyOtpResponse/AuthResponse below.
export interface LoginResendRequest {
  verificationId: string;
}

export interface LoginVerifyRequest {
  verificationId: string;
  code: string;
}

export interface LoginCompleteRequest {
  verificationId: string;
  operationToken: string;
}

// ==================== REGISTER OTP ====================
// Start reuses RegisterRequest above; Resend/Verify/Complete reuse
// StartOtpResponse/ResendOtpResponse/VerifyOtpResponse/AuthResponse below.
export interface RegisterResendRequest {
  verificationId: string;
}

export interface RegisterVerifyRequest {
  verificationId: string;
  code: string;
}

export interface RegisterCompleteRequest {
  verificationId: string;
  operationToken: string;
}

// ==================== CHANGE PHONE ====================
export interface ChangePhoneStartRequest {
  newPhoneNumber: string;
  currentPassword: string;
}

export interface ChangePhoneResendRequest {
  verificationId: string;
}

export interface ChangePhoneVerifyRequest {
  verificationId: string;
  code: string;
}

export interface ChangePhoneCompleteRequest {
  verificationId: string;
  operationToken: string;
}

export interface StartOtpResponse {
  verificationId: string;
  expiresAt: string;
  maxAttempts: number;
}

export interface ResendOtpResponse {
  verificationId: string;
  expiresAt: string;
}

export interface VerifyOtpResponse {
  isVerified: boolean;
  operationToken?: string;
  operationTokenExpiresAt?: string;
}

// ==================== FORGOT PASSWORD ====================
// Start/Resend/Verify reuse StartOtpResponse/ResendOtpResponse/VerifyOtpResponse above -
// only the request shapes and the Complete response differ from Change Phone.
export interface ForgotPasswordStartRequest {
  phoneNumber: string;
}

export interface ForgotPasswordResendRequest {
  verificationId: string;
}

export interface ForgotPasswordVerifyRequest {
  verificationId: string;
  code: string;
}

export interface ForgotPasswordCompleteRequest {
  verificationId: string;
  operationToken: string;
  newPassword: string;
}

export interface ForgotPasswordCompleteResponse {
  success: boolean;
}

// ==================== ENUMS ====================
export enum UserRole {
  Worker = 1,
  Employer = 2,
  Admin = 3,
}

export enum JobStatus {
  Active = 0,
  Closed = 1,
  Draft = 2,
}

export enum ApplicationStatus {
  Pending   = 1,
  Accepted  = 2,
  Rejected  = 3,
  Cancelled = 4,
  Completed = 5,
}

// ==================== JOB ====================
export interface JobImage {
  id: string;
  imageUrl: string;
  caption?: string;
}

export interface Job {
  id: string;
  title: string;
  description: string;
  salary: number;
  jobType: number;
  location?: string;
  city: string;
  country: string;
  latitude: number;
  longitude: number;
  status: JobStatus;
  categoryId?: string;
  categoryName: string;
  subCategoryId?: string;
  subCategoryName?: string;
  employerId: string;
  employerName?: string;
  requiredSkills?: string;
  experienceYears?: number;
  deadline?: string;
  createdAt: string;
  distance?: number;
  // Cheap thumbnail derived server-side; always present on detail responses, present but possibly
  // null on list/search/my-jobs responses (see JobResponseDto.CoverImageUrl on the backend).
  coverImageUrl?: string | null;
  // Full gallery; only populated by detail/update responses - list/search/my-jobs responses leave
  // this empty or absent, so treat a missing/empty array as "not loaded", not "no images".
  images?: JobImage[];
}

export interface JobListResponse {
  items: Job[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
}

// ==================== APPLICATION ====================
export interface JobApplication {
  id: string;
  jobId: string;
  jobTitle: string;
  workerId: string;
  status: ApplicationStatus;
  coverLetter: string;
  createdAt: string;
  workerName?: string;
  workerCvUrl?: string | null;
  workerPhone?: string;
}

// ==================== USER ====================
export interface UserProfile {
  id: string;
  firstName: string;
  lastName: string;
  age: number;
  phoneNumber: string;
  role: UserRole;
  profileImageUrl: string | null;
  latitude: number | null;
  longitude: number | null;
  city: string | null;
  country: string | null;
  balance: number;
  isVerified: boolean;
  createdAt: string;
  cvUrl?: string | null;
}

// ==================== CATEGORY ====================
export interface Category {
  id: string;
  name: string;
  description?: string;
  iconUrl?: string;
  subCategories?: Category[];
}

// ==================== REVIEW ====================
export interface Review {
  id: string;
  rating: number;
  comment: string;
  reviewerName: string;
  createdAt: string;
}

// ==================== API RESPONSE ====================
export interface ApiResponse<T> {
  data: T;
  message: string;
  success: boolean;
}

export enum NotificationType {
  ApplicationAccepted  = 1,
  ApplicationRejected  = 2,
  JobCompleted         = 3,
  ReviewRequest        = 4,
  NewJobRecommended    = 5,
}

// ==================== WORKER POST ====================
export enum WorkerPostStatus {
  Active = 1,
  Inactive = 2,
  Hired = 3,
}

export interface PortfolioImage {
  id: string;
  imageUrl: string;
  caption?: string;
}

export interface WorkerPost {
  id: string;
  title: string;
  description: string;
  expectedSalary: number;
  experienceYears: number;
  skills?: string;
  city: string;
  country: string;
  status: WorkerPostStatus;
  workerId: string;
  workerFirstName: string;
  workerLastName: string;
  workerAvatarUrl?: string;
  workerPhone?: string;        // ← shu qatorni qo'shing
  categoryId?: string;
  categoryName?: string;
  subCategoryId?: string;
  subCategoryName?: string;
  createdAt: string;
  portfolioImages: PortfolioImage[];
  viewCount: number;
}

export interface CreateWorkerPostRequest {
  title: string;
  description: string;
  expectedSalary: number;
  experienceYears: number;
  skills?: string;
  city: string;
  country: string;
  categoryId?: string;
  subCategoryId?: string;
}

export interface UpdateWorkerPostRequest {
  title: string;
  description: string;
  expectedSalary: number;
  experienceYears: number;
  skills?: string;
  city: string;
  country: string;
  categoryId?: string;
  subCategoryId?: string;
  status: number;
}

// ==================== NEARBY JOB ====================
export interface NearbyJob {
  id: string;
  title: string;
  description: string;
  salary: number;
  jobType: number;
  status: JobStatus;
  categoryName: string;
  latitude: number;
  longitude: number;
  city: string;
  country: string;
  employerId: string;
  createdAt: string;
  distanceKm: number;
}

// ==================== PROPERTY ====================
// Numeric values must exactly match the backend enums (Labora.Domain/Enums/PropertyType.cs,
// RenovationStatus.cs, RentalPeriod.cs, PropertyListingStatus.cs).
export enum PropertyType {
  House = 1,
  Apartment = 2,
}

export enum RenovationStatus {
  NeedsRenovation = 1,
  Average = 2,
  Good = 3,
  EuroRenovated = 4,
  NewlyBuilt = 5,
}

export enum RentalPeriod {
  Monthly = 1,
  Daily = 2,
}

export enum PropertyListingStatus {
  Published = 1,
  Archived = 2,
}

export interface PropertyImage {
  id: string;
  imageUrl: string;
  sortOrder: number;
}

// Mirrors PropertyListingResponseDto. FloorNumber/TotalFloors are only ever set for Apartment.
export interface PropertyListing {
  id: string;
  title: string;
  description: string;
  propertyType: PropertyType;
  roomCount: number;
  areaSquareMeters: number;
  floorNumber?: number | null;
  totalFloors?: number | null;
  renovationStatus: RenovationStatus;
  price: number;
  rentalPeriod: RentalPeriod;
  address: string;
  latitude: number;
  longitude: number;
  contactPhoneNumber: string;
  status: PropertyListingStatus;
  ownerId: string;
  // Cheap thumbnail; always set on detail responses, may be null on list/marker-adjacent responses.
  coverImageUrl?: string | null;
  createdAt: string;
  // Full ordered gallery - only populated by the detail fetch, empty on list responses.
  images: PropertyImage[];
}

// Lightweight marker projection for the map screen - mirrors PropertyMarkerDto.
export interface PropertyMarker {
  id: string;
  latitude: number;
  longitude: number;
  price: number;
  propertyType: PropertyType;
}

// Scalar fields only, mirroring PropertyListingRequestDto - image URIs are passed separately to
// propertyService.createProperty, matching how the backend keeps IFormFile out of this shape.
export interface CreatePropertyRequest {
  title: string;
  description: string;
  propertyType: PropertyType;
  roomCount: number;
  areaSquareMeters: number;
  floorNumber?: number | null;
  totalFloors?: number | null;
  renovationStatus: RenovationStatus;
  price: number;
  rentalPeriod: RentalPeriod;
  address: string;
  latitude: number;
  longitude: number;
  contactPhoneNumber: string;
}

// Mirrors ReorderPropertyImagesRequestDto - the full ordered list of a property's currently
// active image ids, sent to PUT /PropertyListing/{id}/images/order.
export interface ReorderPropertyImagesRequest {
  imageIds: string[];
}