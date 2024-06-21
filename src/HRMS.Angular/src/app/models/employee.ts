/**
 *             string FirstName,
            string LastName,
            string Email,
            string Address,
            string Phone,
            string Country,

            Qualification
                        string Title,
            string Description,
            string QualificationType,
            string EducationLevel,
            DateTime? DateReceived,
            DateTime? ExpiryDate
*/

export interface EmployeeBase{
    firstName: string,
    lastName: string,
    email: string,
    address: string,
    phone: string,
    country: string,
    position: string,
    department: string
}

export interface Employee extends EmployeeBase{
    id: number,
    staffId: string
}

export interface CreateEmployee extends EmployeeBase{
}

export interface EditEmployee extends EmployeeBase{
    employeeId: number
}

export interface EmployeeDetail extends Employee{
    qualifications: Qualification[]
}

export interface AddQualification{
    employeeId: number,
    title: string,
    description: string,
    qualificationType: number, //Certification, Education
    educationLevel: number,
    dateReceived: Date|null,
    expiryDate: Date|null,
    mediaFile: any
}

export interface Qualification{
    id: number,
    title: string,
    description: string,
    qualificationType: string, //Certification, Education
    educationLevel: string,
    dateReceived: Date|null,
    expiryDate: Date|null,
    mediaFileId: string|null,
    mediaFile: string|null
}