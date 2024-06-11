/**
 * VTC Staff API
 * Violation Ticket Centre Staff API
 *
 * The version of the OpenAPI document: v1
 * 
 *
 * NOTE: This class is auto generated by OpenAPI Generator (https://openapi-generator.tech).
 * https://openapi-generator.tech
 * Do not edit the class manually.
 */
import { JJDisputeSignatoryType } from './jJDisputeSignatoryType.model';
import { JJDisputeRemark } from './jJDisputeRemark.model';
import { JJDisputeNoticeOfHearingYn } from './jJDisputeNoticeOfHearingYn.model';
import { JJDisputeDisputantAttendanceType } from './jJDisputeDisputantAttendanceType.model';
import { FileMetadata } from './fileMetadata.model';
import { JJDisputeHearingType } from './jJDisputeHearingType.model';
import { JJDisputeElectronicTicketYn } from './jJDisputeElectronicTicketYn.model';
import { JJDisputeAppearInCourt } from './jJDisputeAppearInCourt.model';
import { JJDisputeAccidentYn } from './jJDisputeAccidentYn.model';
import { JJDisputeCourtAppearanceRoP } from './jJDisputeCourtAppearanceRoP.model';
import { JJDisputedCount } from './jJDisputedCount.model';
import { JJDisputeContactType } from './jJDisputeContactType.model';
import { JJDisputeMultipleOfficersYn } from './jJDisputeMultipleOfficersYn.model';
import { JJDisputeStatus } from './jJDisputeStatus.model';


export interface JJDispute { 
    fileData?: Array<FileMetadata> | null;
    lockId?: string | null;
    lockedBy?: string | null;
    lockExpiresAtUtc?: string | null;
    createdBy?: string | null;
    createdTs?: string;
    modifiedBy?: string | null;
    modifiedTs?: string | null;
    id?: number;
    ticketNumber?: string | null;
    accidentYn?: JJDisputeAccidentYn;
    addressLine1?: string | null;
    addressLine2?: string | null;
    addressLine3?: string | null;
    addressCity?: string | null;
    addressProvince?: string | null;
    addressCountry?: string | null;
    addressPostalCode?: string | null;
    disputantBirthdate?: string | null;
    driversLicenceNumber?: string | null;
    drvLicIssuedProvSeqNo?: string | null;
    drvLicIssuedCtryId?: string | null;
    emailAddress?: string | null;
    status?: JJDisputeStatus;
    hearingType?: JJDisputeHearingType;
    multipleOfficersYn?: JJDisputeMultipleOfficersYn;
    noticeOfDisputeGuid?: string | null;
    noticeOfHearingYn?: JJDisputeNoticeOfHearingYn;
    occamDisputantGiven1Nm?: string | null;
    occamDisputantGiven2Nm?: string | null;
    occamDisputantGiven3Nm?: string | null;
    occamDisputantSurnameNm?: string | null;
    occamDisputeId?: number;
    occamViolationTicketUpldId?: string | null;
    submittedTs?: string | null;
    issuedTs?: string | null;
    violationDate?: string | null;
    icbcReceivedDate?: string | null;
    enforcementOfficer?: string | null;
    electronicTicketYn?: JJDisputeElectronicTicketYn;
    policeDetachment?: string | null;
    courthouseLocation?: string | null;
    offenceLocation?: string | null;
    jjAssignedTo?: string | null;
    decisionMadeBy?: string | null;
    jjDecisionDate?: string | null;
    vtcAssignedTo?: string | null;
    vtcAssignedTs?: string | null;
    fineReductionReason?: string | null;
    timeToPayReason?: string | null;
    contactLawFirmName?: string | null;
    contactGivenName1?: string | null;
    contactGivenName2?: string | null;
    contactGivenName3?: string | null;
    contactSurname?: string | null;
    contactType?: JJDisputeContactType;
    appearInCourt?: JJDisputeAppearInCourt;
    courtAgenId?: string | null;
    recalled?: boolean | null;
    lawFirmName?: string | null;
    lawyerSurname?: string | null;
    lawyerGivenName1?: string | null;
    lawyerGivenName2?: string | null;
    lawyerGivenName3?: string | null;
    justinRccId?: string | null;
    interpreterLanguageCd?: string | null;
    witnessNo?: number | null;
    disputantAttendanceType?: JJDisputeDisputantAttendanceType;
    signatoryType?: JJDisputeSignatoryType;
    signatoryName?: string | null;
    remarks?: Array<JJDisputeRemark> | null;
    jjDisputedCounts?: Array<JJDisputedCount> | null;
    jjDisputeCourtAppearanceRoPs?: Array<JJDisputeCourtAppearanceRoP> | null;
    additionalProperties?: { [key: string]: any; } | null;
}

