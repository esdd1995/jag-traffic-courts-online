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
import { TicketImageDataJustinDocumentReportType } from './ticketImageDataJustinDocumentReportType.model';


export interface TicketImageDataJustinDocument { 
    reportType?: TicketImageDataJustinDocumentReportType;
    reportFormat?: string | null;
    partId?: string | null;
    participantName?: string | null;
    index?: string | null;
    fileData?: string | null;
    additionalProperties?: { [key: string]: any; } | null;
}

