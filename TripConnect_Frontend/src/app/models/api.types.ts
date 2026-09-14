// Shared API types — single source of truth for all backend DTOs

// ─── Auth ────────────────────────────────────────────────────────────────────

export interface LoginUserDto {
  username: string;
  password: string;
}

export interface CreateUserDto {
  name: string;
  email: string;
  username: string;
  phone: string;
  password: string;
}

export interface JwtTokenResponseDto {
  accessToken: string;
  refreshToken: string;
  tokenType: string;
  expiresIn: number;
  issuedAt: string;
  expiresAt: string;
}

export interface AuthResponseDto {
  success: boolean;
  message: string;
  user: UserResponseDto;
  token: JwtTokenResponseDto;
}

// ─── User ─────────────────────────────────────────────────────────────────────

export interface UserResponseDto {
  id: number;
  name: string;
  username: string;
  email: string;
  phone: string;
  rating: number;
  phoneVerified: boolean;
  idVerified: boolean;
  emailVerified: boolean;
  createdAt: string;
}

export interface UserFullDto extends UserResponseDto {
  avatarUrl?: string;
  averageRating?: number;
  totalRatings?: number;
}

export interface UpdateUserDto {
  name: string;
  phone: string;
}

// ─── Trip ─────────────────────────────────────────────────────────────────────

export interface TripDayDto {
  day: number;
  location?: string;
  date: string;        // "YYYY-MM-DD"
  description?: string;
  imgUrl?: string;
  latitude?: number;
  longitude?: number;
}

export interface TripDayResponseDto {
  id: number;
  tripId: number;
  day: number;
  location?: string;
  date: string;        // "YYYY-MM-DD"
  description?: string;
  imgUrl?: string;
  latitude?: number;
  longitude?: number;
}

export interface TripResponseDto {
  id: number;
  title: string;
  description: string;
  location: string;
  budget: number;
  startDate: string;
  endDate: string;
  seats: number;
  travelType: string;
  status: string;
  hostId: number;
  hostName: string | null;
  hostRating: number;
  memberCount: number;
  filledSeats: number;
  openSeats: number;
  imgUrl: string;
  createdAt: string;
  tripDays: TripDayResponseDto[];
}

export interface CreateTripDto {
  title: string;
  description: string;
  location: string;
  budget: number;
  startDate: string;
  endDate: string;
  seats: number;
  travelType: string;
  ImgUrl: string;
  tripDays?: TripDayDto[];
}

export interface UpdateTripDto {
  title: string;
  description: string;
  location: string;
  budget: number;
  startDate: string;
  endDate: string;
  seats: number;
  travelType: string;
  ImgUrl?: string;
  tripDays?: TripDayDto[];
}

export interface TripSearchFilters {
  location?: string;
  startDate?: string;
  endDate?: string;
  minBudget?: number;
  maxBudget?: number;
  travelType?: string;
}

// ─── Trip Member ──────────────────────────────────────────────────────────────

export interface TripMemberResponseDto {
  id: number;
  userId: number;
  userName: string;
  role: string;
  joinedAt: string;
}

// ─── Join Request ─────────────────────────────────────────────────────────────

export interface SendJoinRequestDto {
  tripId: number;
}

export interface JoinRequestResponseDto {
  id: number;
  tripId: number;
  userId: number;
  userName: string;
  userUsername?: string;
  userAvatar?: string;
  status: 'Pending' | 'Accepted' | 'Rejected' | 'Cancelled';
  requestedAt: string;
  respondedAt?: string | null;
}

// ─── Rating / Review ──────────────────────────────────────────────────────────

export interface CreateRatingDto {
  tripId: number;
  ratedUserId: number;
  rating: number;
  review: string;
}

export interface RatingResponseDto {
  id: number;
  tripId: number;
  tripTitle: string;
  raterUserId: number;
  raterUserName: string;
  ratedUserId: number;
  rating: number;
  review: string;
  createdAt: string;
}
