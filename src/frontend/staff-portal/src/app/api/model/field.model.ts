/**
 * VTC Staff API
 *
 * 
 *
 * NOTE: This class is auto generated by OpenAPI Generator (https://openapi-generator.tech).
 * https://openapi-generator.tech
 * Do not edit the class manually.
 */
import { BoundingBox } from './boundingBox.model';


export interface Field { 
    value?: string | null;
    fieldConfidence?: number | null;
    validationErrors?: Array<string> | null;
    type?: string | null;
    boundingBoxes?: Array<BoundingBox> | null;
}

