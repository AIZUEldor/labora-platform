import Svg, { Path, Circle, Rect, Line } from 'react-native-svg';

interface IconProps {
  size?: number;
  color?: string;
  strokeWidth?: number;
}

export function HomeIcon({ size = 24, color = '#000', strokeWidth = 1.8 }: IconProps) {
  return (
    <Svg width={size} height={size} viewBox="0 0 24 24" fill="none">
      <Path d="M3 9.5L12 3L21 9.5V20C21 20.55 20.55 21 20 21H15V15H9V21H4C3.45 21 3 20.55 3 20V9.5Z" stroke={color} strokeWidth={strokeWidth} strokeLinejoin="round" />
    </Svg>
  );
}

export function ApplicationsIcon({ size = 24, color = '#000', strokeWidth = 1.8 }: IconProps) {
  return (
    <Svg width={size} height={size} viewBox="0 0 24 24" fill="none">
      <Rect x="4" y="3" width="16" height="18" rx="2" stroke={color} strokeWidth={strokeWidth} />
      <Path d="M8 8H16M8 12H16M8 16H12" stroke={color} strokeWidth={strokeWidth} strokeLinecap="round" />
    </Svg>
  );
}

export function ProfileIcon({ size = 24, color = '#000', strokeWidth = 1.8 }: IconProps) {
  return (
    <Svg width={size} height={size} viewBox="0 0 24 24" fill="none">
      <Circle cx="12" cy="8" r="4" stroke={color} strokeWidth={strokeWidth} />
      <Path d="M4 20C4 17 7.58 15 12 15C16.42 15 20 17 20 20" stroke={color} strokeWidth={strokeWidth} strokeLinecap="round" />
    </Svg>
  );
}

export function EditIcon({ size = 24, color = '#000', strokeWidth = 1.8 }: IconProps) {
  return (
    <Svg width={size} height={size} viewBox="0 0 24 24" fill="none">
      <Path d="M11 4H4C3.45 4 3 4.45 3 5V20C3 20.55 3.45 21 4 21H19C19.55 21 20 20.55 20 20V13" stroke={color} strokeWidth={strokeWidth} strokeLinecap="round" />
      <Path d="M18.5 2.5C19.33 1.67 20.67 1.67 21.5 2.5C22.33 3.33 22.33 4.67 21.5 5.5L12 15L8 16L9 12L18.5 2.5Z" stroke={color} strokeWidth={strokeWidth} strokeLinejoin="round" />
    </Svg>
  );
}

export function HeartIcon({ size = 24, color = '#000', strokeWidth = 1.8 }: IconProps) {
  return (
    <Svg width={size} height={size} viewBox="0 0 24 24" fill="none">
      <Path d="M12 21C12 21 3 14.5 3 8.5C3 5.42 5.42 3 8.5 3C10.24 3 11.91 3.81 13 5.08C14.09 3.81 15.76 3 17.5 3C20.58 3 23 5.42 23 8.5C23 14.5 12 21 12 21Z" stroke={color} strokeWidth={strokeWidth} strokeLinejoin="round" />
    </Svg>
  );
}

export function StarIcon({ size = 24, color = '#000', strokeWidth = 1.8 }: IconProps) {
  return (
    <Svg width={size} height={size} viewBox="0 0 24 24" fill="none">
      <Path d="M12 2L15.09 8.26L22 9.27L17 14.14L18.18 21.02L12 17.77L5.82 21.02L7 14.14L2 9.27L8.91 8.26L12 2Z" stroke={color} strokeWidth={strokeWidth} strokeLinejoin="round" />
    </Svg>
  );
}

export function BellIcon({ size = 24, color = '#000', strokeWidth = 1.8 }: IconProps) {
  return (
    <Svg width={size} height={size} viewBox="0 0 24 24" fill="none">
      <Path d="M18 8A6 6 0 0 0 6 8C6 15 3 17 3 17H21C21 17 18 15 18 8Z" stroke={color} strokeWidth={strokeWidth} strokeLinejoin="round" />
      <Path d="M13.73 21C13.55 21.3 13.3 21.55 13 21.73C12.7 21.91 12.35 22 12 22C11.65 22 11.3 21.91 11 21.73C10.7 21.55 10.45 21.3 10.27 21" stroke={color} strokeWidth={strokeWidth} strokeLinecap="round" />
    </Svg>
  );
}

export function LockIcon({ size = 24, color = '#000', strokeWidth = 1.8 }: IconProps) {
  return (
    <Svg width={size} height={size} viewBox="0 0 24 24" fill="none">
      <Rect x="3" y="11" width="18" height="11" rx="2" stroke={color} strokeWidth={strokeWidth} />
      <Path d="M7 11V7C7 4.24 9.24 2 12 2C14.76 2 17 4.24 17 7V11" stroke={color} strokeWidth={strokeWidth} strokeLinecap="round" />
      <Circle cx="12" cy="16" r="1" fill={color} />
    </Svg>
  );
}

export function HelpIcon({ size = 24, color = '#000', strokeWidth = 1.8 }: IconProps) {
  return (
    <Svg width={size} height={size} viewBox="0 0 24 24" fill="none">
      <Circle cx="12" cy="12" r="10" stroke={color} strokeWidth={strokeWidth} />
      <Path d="M9.09 9C9.33 8.34 9.81 7.8 10.43 7.46C11.05 7.12 11.77 7.01 12.48 7.14C13.19 7.28 13.83 7.65 14.28 8.19C14.73 8.73 14.96 9.41 14.94 10.11C14.94 12 12 13 12 13" stroke={color} strokeWidth={strokeWidth} strokeLinecap="round" />
      <Circle cx="12" cy="17" r="0.5" fill={color} stroke={color} />
    </Svg>
  );
}

export function LogoutIcon({ size = 24, color = '#000', strokeWidth = 1.8 }: IconProps) {
  return (
    <Svg width={size} height={size} viewBox="0 0 24 24" fill="none">
      <Path d="M9 21H5C4.47 21 3.96 20.79 3.59 20.41C3.21 20.04 3 19.53 3 19V5C3 4.47 3.21 3.96 3.59 3.59C3.96 3.21 4.47 3 5 3H9" stroke={color} strokeWidth={strokeWidth} strokeLinecap="round" strokeLinejoin="round" />
      <Path d="M16 17L21 12L16 7" stroke={color} strokeWidth={strokeWidth} strokeLinecap="round" strokeLinejoin="round" />
      <Line x1="21" y1="12" x2="9" y2="12" stroke={color} strokeWidth={strokeWidth} strokeLinecap="round" />
    </Svg>
  );
}

export function SunIcon({ size = 24, color = '#000', strokeWidth = 1.8 }: IconProps) {
  return (
    <Svg width={size} height={size} viewBox="0 0 24 24" fill="none">
      <Circle cx="12" cy="12" r="5" stroke={color} strokeWidth={strokeWidth} />
      <Path d="M12 2V4M12 20V22M4.22 4.22L5.64 5.64M18.36 18.36L19.78 19.78M2 12H4M20 12H22M4.22 19.78L5.64 18.36M18.36 5.64L19.78 4.22" stroke={color} strokeWidth={strokeWidth} strokeLinecap="round" />
    </Svg>
  );
}

export function MoonIcon({ size = 24, color = '#000', strokeWidth = 1.8 }: IconProps) {
  return (
    <Svg width={size} height={size} viewBox="0 0 24 24" fill="none">
      <Path d="M21 12.79A9 9 0 1 1 11.21 3 7 7 0 0 0 21 12.79Z" stroke={color} strokeWidth={strokeWidth} strokeLinejoin="round" />
    </Svg>
  );
}

export function ChevronRightIcon({ size = 24, color = '#000', strokeWidth = 1.8 }: IconProps) {
  return (
    <Svg width={size} height={size} viewBox="0 0 24 24" fill="none">
      <Path d="M9 18L15 12L9 6" stroke={color} strokeWidth={strokeWidth} strokeLinecap="round" strokeLinejoin="round" />
    </Svg>
  );
}

export function CameraIcon({ size = 24, color = '#000', strokeWidth = 1.8 }: IconProps) {
  return (
    <Svg width={size} height={size} viewBox="0 0 24 24" fill="none">
      <Path d="M23 19C23 19.53 22.79 20.04 22.41 20.41C22.04 20.79 21.53 21 21 21H3C2.47 21 1.96 20.79 1.59 20.41C1.21 20.04 1 19.53 1 19V8C1 7.47 1.21 6.96 1.59 6.59C1.96 6.21 2.47 6 3 6H7L9 3H15L17 6H21C21.53 6 22.04 6.21 22.41 6.59C22.79 6.96 23 7.47 23 8V19Z" stroke={color} strokeWidth={strokeWidth} strokeLinejoin="round" />
      <Circle cx="12" cy="13" r="4" stroke={color} strokeWidth={strokeWidth} />
    </Svg>
  );
}

export function ConstructionIcon({ size = 24, color = '#000', strokeWidth = 1.8 }: IconProps) {
  return (
    <Svg width={size} height={size} viewBox="0 0 24 24" fill="none">
      <Path d="M2 20H22M6 20V10M18 20V10M10 20V14H14V20M6 10L12 4L18 10" stroke={color} strokeWidth={strokeWidth} strokeLinecap="round" strokeLinejoin="round" />
    </Svg>
  );
}

export function ITIcon({ size = 24, color = '#000', strokeWidth = 1.8 }: IconProps) {
  return (
    <Svg width={size} height={size} viewBox="0 0 24 24" fill="none">
      <Rect x="2" y="3" width="20" height="14" rx="2" stroke={color} strokeWidth={strokeWidth} />
      <Path d="M8 21H16M12 17V21" stroke={color} strokeWidth={strokeWidth} strokeLinecap="round" />
      <Path d="M8 10L10 12L8 14M12 14H16" stroke={color} strokeWidth={strokeWidth} strokeLinecap="round" strokeLinejoin="round" />
    </Svg>
  );
}

export function DriverIcon({ size = 24, color = '#000', strokeWidth = 1.8 }: IconProps) {
  return (
    <Svg width={size} height={size} viewBox="0 0 24 24" fill="none">
      <Path d="M5 17H3V12L5 6H19L21 12V17H19M5 17H19M5 17C5 18.1 5.9 19 7 19C8.1 19 9 18.1 9 17M19 17C19 18.1 18.1 19 17 19C15.9 19 15 18.1 15 17M9 17H15" stroke={color} strokeWidth={strokeWidth} strokeLinecap="round" strokeLinejoin="round" />
      <Path d="M6 12H18" stroke={color} strokeWidth={strokeWidth} strokeLinecap="round" />
    </Svg>
  );
}

export function ChefIcon({ size = 24, color = '#000', strokeWidth = 1.8 }: IconProps) {
  return (
    <Svg width={size} height={size} viewBox="0 0 24 24" fill="none">
      <Path d="M6 13.87A4 4 0 0 1 7.41 6a5.11 5.11 0 0 1 1.05-1.54 5 5 0 0 1 7.08 0A5.11 5.11 0 0 1 16.59 6 4 4 0 0 1 18 13.87V21H6V13.87Z" stroke={color} strokeWidth={strokeWidth} strokeLinejoin="round" />
      <Path d="M6 17H18" stroke={color} strokeWidth={strokeWidth} strokeLinecap="round" />
    </Svg>
  );
}

export function MedicalIcon({ size = 24, color = '#000', strokeWidth = 1.8 }: IconProps) {
  return (
    <Svg width={size} height={size} viewBox="0 0 24 24" fill="none">
      <Rect x="3" y="3" width="18" height="18" rx="2" stroke={color} strokeWidth={strokeWidth} />
      <Path d="M12 8V16M8 12H16" stroke={color} strokeWidth={strokeWidth} strokeLinecap="round" />
    </Svg>
  );
}

export function EducationIcon({ size = 24, color = '#000', strokeWidth = 1.8 }: IconProps) {
  return (
    <Svg width={size} height={size} viewBox="0 0 24 24" fill="none">
      <Path d="M2 10L12 4L22 10L12 16L2 10Z" stroke={color} strokeWidth={strokeWidth} strokeLinejoin="round" />
      <Path d="M6 12V17C6 17 8 20 12 20C16 20 18 17 18 17V12" stroke={color} strokeWidth={strokeWidth} strokeLinecap="round" strokeLinejoin="round" />
      <Path d="M22 10V16" stroke={color} strokeWidth={strokeWidth} strokeLinecap="round" />
    </Svg>
  );
}

export function FinanceIcon({ size = 24, color = '#000', strokeWidth = 1.8 }: IconProps) {
  return (
    <Svg width={size} height={size} viewBox="0 0 24 24" fill="none">
      <Path d="M12 2C6.48 2 2 6.48 2 12C2 17.52 6.48 22 12 22C17.52 22 22 17.52 22 12C22 6.48 17.52 2 12 2Z" stroke={color} strokeWidth={strokeWidth} />
      <Path d="M12 6V8M12 16V18M9 9.5C9 8.67 9.67 8 10.5 8H13C13.83 8 14.5 8.67 14.5 9.5C14.5 10.33 13.83 11 13 11H11C10.17 11 9.5 11.67 9.5 12.5C9.5 13.33 10.17 14 11 14H13.5C14.33 14 15 13.33 15 12.5" stroke={color} strokeWidth={strokeWidth} strokeLinecap="round" />
    </Svg>
  );
}

export function SecurityIcon({ size = 24, color = '#000', strokeWidth = 1.8 }: IconProps) {
  return (
    <Svg width={size} height={size} viewBox="0 0 24 24" fill="none">
      <Path d="M12 22C12 22 4 18 4 12V5L12 2L20 5V12C20 18 12 22 12 22Z" stroke={color} strokeWidth={strokeWidth} strokeLinejoin="round" />
      <Path d="M9 12L11 14L15 10" stroke={color} strokeWidth={strokeWidth} strokeLinecap="round" strokeLinejoin="round" />
    </Svg>
  );
}

export function CleaningIcon({ size = 24, color = '#000', strokeWidth = 1.8 }: IconProps) {
  return (
    <Svg width={size} height={size} viewBox="0 0 24 24" fill="none">
      <Path d="M9 3H15L13 9H11L9 3Z" stroke={color} strokeWidth={strokeWidth} strokeLinejoin="round" />
      <Path d="M11 9H13V14H11V9Z" stroke={color} strokeWidth={strokeWidth} strokeLinejoin="round" />
      <Path d="M7 14H17C17 14 18 21 12 21C6 21 7 14 7 14Z" stroke={color} strokeWidth={strokeWidth} strokeLinejoin="round" />
    </Svg>
  );
}

export function DesignIcon({ size = 24, color = '#000', strokeWidth = 1.8 }: IconProps) {
  return (
    <Svg width={size} height={size} viewBox="0 0 24 24" fill="none">
      <Circle cx="12" cy="12" r="10" stroke={color} strokeWidth={strokeWidth} />
      <Circle cx="12" cy="12" r="3" stroke={color} strokeWidth={strokeWidth} />
      <Path d="M12 2V5M12 19V22M2 12H5M19 12H22" stroke={color} strokeWidth={strokeWidth} strokeLinecap="round" />
    </Svg>
  );
}

export function MarketingIcon({ size = 24, color = '#000', strokeWidth = 1.8 }: IconProps) {
  return (
    <Svg width={size} height={size} viewBox="0 0 24 24" fill="none">
      <Path d="M18 20V10M12 20V4M6 20V14" stroke={color} strokeWidth={strokeWidth} strokeLinecap="round" strokeLinejoin="round" />
    </Svg>
  );
}

export function WarehouseIcon({ size = 24, color = '#000', strokeWidth = 1.8 }: IconProps) {
  return (
    <Svg width={size} height={size} viewBox="0 0 24 24" fill="none">
      <Path d="M2 20V8L12 3L22 8V20H2Z" stroke={color} strokeWidth={strokeWidth} strokeLinejoin="round" />
      <Path d="M9 20V14H15V20" stroke={color} strokeWidth={strokeWidth} strokeLinejoin="round" />
      <Path d="M2 8H22" stroke={color} strokeWidth={strokeWidth} />
    </Svg>
  );
}

export function MapPinIcon({ size = 24, color = '#000', strokeWidth = 1.8 }: IconProps) {
  return (
    <Svg width={size} height={size} viewBox="0 0 24 24" fill="none">
      <Path d="M12 2C8.69 2 6 4.69 6 8C6 12.5 12 22 12 22C12 22 18 12.5 18 8C18 4.69 15.31 2 12 2Z" stroke={color} strokeWidth={strokeWidth} strokeLinejoin="round" />
      <Circle cx="12" cy="8" r="3" stroke={color} strokeWidth={strokeWidth} />
    </Svg>
  );
}

export function ClockIcon({ size = 24, color = '#000', strokeWidth = 1.8 }: IconProps) {
  return (
    <Svg width={size} height={size} viewBox="0 0 24 24" fill="none">
      <Circle cx="12" cy="12" r="10" stroke={color} strokeWidth={strokeWidth} />
      <Path d="M12 6V12L16 14" stroke={color} strokeWidth={strokeWidth} strokeLinecap="round" strokeLinejoin="round" />
    </Svg>
  );
}

export function InsuranceIcon({ size = 24, color = '#000', strokeWidth = 1.8 }: IconProps) {
  return (
    <Svg width={size} height={size} viewBox="0 0 24 24" fill="none">
      <Path d="M12 22C12 22 4 18 4 12V5L12 2L20 5V12C20 18 12 22 12 22Z" stroke={color} strokeWidth={strokeWidth} strokeLinejoin="round" />
      <Path d="M9 12L11 14L15 10" stroke={color} strokeWidth={strokeWidth} strokeLinecap="round" strokeLinejoin="round" />
    </Svg>
  );
}

export function TrainingIcon({ size = 24, color = '#000', strokeWidth = 1.8 }: IconProps) {
  return (
    <Svg width={size} height={size} viewBox="0 0 24 24" fill="none">
      <Path d="M2 10L12 4L22 10L12 16L2 10Z" stroke={color} strokeWidth={strokeWidth} strokeLinejoin="round" />
      <Path d="M6 12V17C6 17 8 20 12 20C16 20 18 17 18 17V12" stroke={color} strokeWidth={strokeWidth} strokeLinecap="round" strokeLinejoin="round" />
    </Svg>
  );
}

export function TransportIcon({ size = 24, color = '#000', strokeWidth = 1.8 }: IconProps) {
  return (
    <Svg width={size} height={size} viewBox="0 0 24 24" fill="none">
      <Path d="M5 17H3V12L5 6H19L21 12V17H19M5 17H19M5 17C5 18.1 5.9 19 7 19C8.1 19 9 18.1 9 17M19 17C19 18.1 18.1 19 17 19C15.9 19 15 18.1 15 17M9 17H15" stroke={color} strokeWidth={strokeWidth} strokeLinecap="round" strokeLinejoin="round" />
    </Svg>
  );
}

export function FoodIcon({ size = 24, color = '#000', strokeWidth = 1.8 }: IconProps) {
  return (
    <Svg width={size} height={size} viewBox="0 0 24 24" fill="none">
      <Path d="M18 8H19C20.1 8 21 8.9 21 10V14C21 15.1 20.1 16 19 16H18" stroke={color} strokeWidth={strokeWidth} strokeLinecap="round" />
      <Path d="M3 8H18V17C18 18.1 17.1 19 16 19H5C3.9 19 3 18.1 3 17V8Z" stroke={color} strokeWidth={strokeWidth} strokeLinejoin="round" />
      <Path d="M6 8V6C6 4.9 6.9 4 8 4H16C17.1 4 18 4.9 18 6V8" stroke={color} strokeWidth={strokeWidth} />
    </Svg>
  );
}

export function CheckCircleIcon({ size = 24, color = '#000', strokeWidth = 1.8 }: IconProps) {
  return (
    <Svg width={size} height={size} viewBox="0 0 24 24" fill="none">
      <Circle cx="12" cy="12" r="10" stroke={color} strokeWidth={strokeWidth} />
      <Path d="M9 12L11 14L15 10" stroke={color} strokeWidth={strokeWidth} strokeLinecap="round" strokeLinejoin="round" />
    </Svg>
  );
}

export function MoneyIcon({ size = 24, color = '#000', strokeWidth = 1.8 }: IconProps) {
  return (
    <Svg width={size} height={size} viewBox="0 0 24 24" fill="none">
      <Rect x="2" y="6" width="20" height="12" rx="2" stroke={color} strokeWidth={strokeWidth} />
      <Circle cx="12" cy="12" r="3" stroke={color} strokeWidth={strokeWidth} />
      <Path d="M6 12H6.01M18 12H18.01" stroke={color} strokeWidth={strokeWidth} strokeLinecap="round" />
    </Svg>
  );
}

export function SearchIcon({ size = 24, color = '#000', strokeWidth = 1.8 }: IconProps) {
  return (
    <Svg width={size} height={size} viewBox="0 0 24 24" fill="none">
      <Circle cx="11" cy="11" r="8" stroke={color} strokeWidth={strokeWidth} />
      <Path d="M21 21L16.65 16.65" stroke={color} strokeWidth={strokeWidth} strokeLinecap="round" />
    </Svg>
  );
}

export function LocationIcon({ size = 24, color = '#000', strokeWidth = 1.8 }: IconProps) {
  return (
    <Svg width={size} height={size} viewBox="0 0 24 24" fill="none">
      <Path d="M12 2C8.69 2 6 4.69 6 8C6 12.5 12 22 12 22C12 22 18 12.5 18 8C18 4.69 15.31 2 12 2Z" stroke={color} strokeWidth={strokeWidth} strokeLinejoin="round" />
      <Circle cx="12" cy="8" r="3" stroke={color} strokeWidth={strokeWidth} />
    </Svg>
  );
}

export function BriefcaseIcon({ size = 24, color = '#000', strokeWidth = 1.8 }: IconProps) {
  return (
    <Svg width={size} height={size} viewBox="0 0 24 24" fill="none">
      <Rect x="2" y="7" width="20" height="14" rx="2" stroke={color} strokeWidth={strokeWidth} />
      <Path d="M16 7V5C16 3.9 15.1 3 14 3H10C8.9 3 8 3.9 8 5V7" stroke={color} strokeWidth={strokeWidth} strokeLinecap="round" />
      <Path d="M12 12V16M2 12H22" stroke={color} strokeWidth={strokeWidth} strokeLinecap="round" />
    </Svg>
  );
}

export function ArrowLeftIcon({ size = 24, color = '#000', strokeWidth = 1.8 }: IconProps) {
  return (
    <Svg width={size} height={size} viewBox="0 0 24 24" fill="none">
      <Path d="M19 12H5M5 12L12 19M5 12L12 5" stroke={color} strokeWidth={strokeWidth} strokeLinecap="round" strokeLinejoin="round" />
    </Svg>
  );
}

export function CheckIcon({ size = 24, color = '#000', strokeWidth = 1.8 }: IconProps) {
  return (
    <Svg width={size} height={size} viewBox="0 0 24 24" fill="none">
      <Path d="M20 6L9 17L4 12" stroke={color} strokeWidth={strokeWidth} strokeLinecap="round" strokeLinejoin="round" />
    </Svg>
  );
}

export function UserIcon({ size = 24, color = '#000', strokeWidth = 1.8 }: IconProps) {
  return (
    <Svg width={size} height={size} viewBox="0 0 24 24" fill="none">
      <Circle cx="12" cy="8" r="4" stroke={color} strokeWidth={strokeWidth} />
      <Path d="M4 20C4 17 7.58 15 12 15C16.42 15 20 17 20 20" stroke={color} strokeWidth={strokeWidth} strokeLinecap="round" />
    </Svg>
  );
}

export function PhoneIcon({ size = 24, color = '#000', strokeWidth = 1.8 }: IconProps) {
  return (
    <Svg width={size} height={size} viewBox="0 0 24 24" fill="none">
      <Path d="M6.62 10.79C8.06 13.62 10.38 15.93 13.21 17.38L15.41 15.18C15.68 14.91 16.08 14.82 16.43 14.94C17.55 15.31 18.76 15.51 20 15.51C20.55 15.51 21 15.96 21 16.51V20C21 20.55 20.55 21 20 21C10.61 21 3 13.39 3 4C3 3.45 3.45 3 4 3H7.5C8.05 3 8.5 3.45 8.5 4C8.5 5.25 8.7 6.45 9.07 7.57C9.18 7.92 9.1 8.31 8.82 8.59L6.62 10.79Z" stroke={color} strokeWidth={strokeWidth} strokeLinecap="round" strokeLinejoin="round" />
    </Svg>
  );
}

export function EyeIcon({ size = 24, color = '#000', strokeWidth = 1.8 }: IconProps) {
  return (
    <Svg width={size} height={size} viewBox="0 0 24 24" fill="none">
      <Path d="M1 12C1 12 5 4 12 4C19 4 23 12 23 12C23 12 19 20 12 20C5 20 1 12 1 12Z" stroke={color} strokeWidth={strokeWidth} strokeLinejoin="round" />
      <Circle cx="12" cy="12" r="3" stroke={color} strokeWidth={strokeWidth} />
    </Svg>
  );
}

export function EyeOffIcon({ size = 24, color = '#000', strokeWidth = 1.8 }: IconProps) {
  return (
    <Svg width={size} height={size} viewBox="0 0 24 24" fill="none">
      <Path d="M17.94 17.94C16.23 19.24 14.19 20 12 20C5 20 1 12 1 12C2.24 9.92 3.9 8.12 5.84 6.76M9.9 4.24C10.58 4.08 11.29 4 12 4C19 4 23 12 23 12C22.45 13.07 21.78 14.07 21 14.97M3 3L21 21" stroke={color} strokeWidth={strokeWidth} strokeLinecap="round" strokeLinejoin="round" />
      <Path d="M14.12 14.12C13.84 14.41 13.5 14.64 13.12 14.8C12.75 14.96 12.38 15.04 12 15.04C11.12 15.04 10.28 14.69 9.66 14.08C9.04 13.46 8.7 12.62 8.7 11.74C8.7 11.37 8.77 11 8.93 10.63" stroke={color} strokeWidth={strokeWidth} strokeLinecap="round" />
    </Svg>
  );
}