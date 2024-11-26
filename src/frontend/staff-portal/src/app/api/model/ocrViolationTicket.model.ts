/**
 * VTC Staff API
 *
 * 
 *
 * NOTE: This class is auto generated by OpenAPI Generator (https://openapi-generator.tech).
 * https://openapi-generator.tech
 * Do not edit the class manually.
 */
import { Field } from './field.model';
import { ViolationTicketVersion } from './violationTicketVersion.model';


export interface OcrViolationTicket { 
    ticketVersion?: ViolationTicketVersion;
    imageFilename?: string | null;
    globalConfidence?: number;
    fields?: { [key: string]: Field; } | null;
}
export namespace OcrViolationTicket {
}


