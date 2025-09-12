export interface Student {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  dateOfBirth: string;
  nationality: string;
}

export interface Registration {
  id: string;
  studentId: string;
  student: Student;
  academicYear: string;
  semester: string;
  status: RegistrationStatus;
  registrationDate: string;
  courses: Course[];
}

export const RegistrationStatus = {
  Pending: 'Pending',
  Approved: 'Approved',
  Rejected: 'Rejected',
} as const;

export type RegistrationStatus = typeof RegistrationStatus[keyof typeof RegistrationStatus];

export interface Course {
  id: string;
  name: string;
  code: string;
  credits: number;
  semester: string;
}