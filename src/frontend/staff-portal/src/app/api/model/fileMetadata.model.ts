/**
 * VTC Staff API
 *
 * 
 *
 * NOTE: This class is auto generated by OpenAPI Generator (https://openapi-generator.tech).
 * https://openapi-generator.tech
 * Do not edit the class manually.
 */
import { DocumentSource } from './documentSource.model';


export interface FileMetadata { 
    fileId?: string | null;
    fileName?: string | null;
    noticeOfDisputeGuid?: string | null;
    documentSource?: DocumentSource;
    documentType?: string | null;
    virusScanStatus?: string | null;
    documentStatus?: string | null;
    ticketNumber?: string | null;
    disputeId?: number | null;
    pendingFileStream?: string | null;
    deleteRequested?: boolean | null;
}
export namespace FileMetadata {
}


