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