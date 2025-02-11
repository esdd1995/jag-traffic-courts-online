package ca.bc.gov.open.jag.tco.oracledataapi.mapper;

import org.mapstruct.InjectionStrategy;
import org.mapstruct.Mapper;
import org.mapstruct.Mapping;
import org.mapstruct.factory.Mappers;

import ca.bc.gov.open.jag.tco.oracledataapi.model.FileHistory;

/**
 * This mapper maps from ORDS AuditLogEntry model to Oracle Data API FileHistory model and vice versa
 */
@Mapper
(componentModel = "spring", injectionStrategy = InjectionStrategy.CONSTRUCTOR) // This is required for tests to work
public interface AuditLogEntryMapper {

	AuditLogEntryMapper INSTANCE = Mappers.getMapper(AuditLogEntryMapper.class);

	// Map OCCAM AuditLogEntry data from ORDS to Oracle Data API FileHistory model
	@Mapping(source = "auditLogEntryId", target = "fileHistoryId")
	@Mapping(source = "auditLogEntryTypeCd", target = "auditLogEntryType")
	@Mapping(source = "auditLogEntryTypeDsc", target = "description")
	@Mapping(source = "actionByApplicationUser", target = "actionByApplicationUser")
	@Mapping(source = "commentTxt", target = "comment")
	@Mapping(source = "entUserId", target = "createdBy")
	@Mapping(source = "entDtm", target = "createdTs")
	@Mapping(source = "updUserId", target = "modifiedBy")
	@Mapping(source = "updDtm", target = "modifiedTs")
	FileHistory convert(ca.bc.gov.open.jag.tco.oracledataapi.ords.occam.api.model.AuditLogEntry auditLogEntry);

	// Map Oracle Data API FileHistory model to ORDS OCCAM AuditLogEntry data
	@Mapping(target = "auditLogEntryId", source = "fileHistoryId")
	@Mapping(target = "auditLogEntryTypeCd", source = "auditLogEntryType")
	@Mapping(target = "auditLogEntryTypeDsc", source = "description")
	@Mapping(target = "actionByApplicationUser", source = "actionByApplicationUser")
	@Mapping(target = "commentTxt", source = "comment")
	@Mapping(target = "entUserId", source = "createdBy")
	@Mapping(target = "entDtm", source = "createdTs")
	@Mapping(target = "updUserId", source = "modifiedBy")
	@Mapping(target = "updDtm", source = "modifiedTs")
	ca.bc.gov.open.jag.tco.oracledataapi.ords.occam.api.model.AuditLogEntry convertToOccamAuditLogEntry(FileHistory fileHistory);
	
	// Map TCO AuditLogEntry data from ORDS to Oracle Data API FileHistory model
	@Mapping(source = "auditLogEntryId", target = "fileHistoryId")
	@Mapping(source = "auditLogEntryTypeCd", target = "auditLogEntryType")
	@Mapping(source = "auditLogEntryTypeDsc", target = "description")
	@Mapping(source = "actionByApplicationUser", target = "actionByApplicationUser")
	@Mapping(source = "commentTxt", target = "comment")
	@Mapping(source = "entUserId", target = "createdBy")
	@Mapping(source = "entDtm", target = "createdTs")
	@Mapping(source = "updUserId", target = "modifiedBy")
	@Mapping(source = "updDtm", target = "modifiedTs")
	FileHistory convert(ca.bc.gov.open.jag.tco.oracledataapi.ords.tco.api.model.AuditLogEntry auditLogEntry);

	// Map Oracle Data API FileHistory model to ORDS TCO AuditLogEntry data
	@Mapping(target = "auditLogEntryId", source = "fileHistoryId")
	@Mapping(target = "auditLogEntryTypeCd", source = "auditLogEntryType")
	@Mapping(target = "auditLogEntryTypeDsc", source = "description")
	@Mapping(target = "actionByApplicationUser", source = "actionByApplicationUser")
	@Mapping(target = "commentTxt", source = "comment")
	@Mapping(target = "entUserId", source = "createdBy")
	@Mapping(target = "entDtm", source = "createdTs")
	@Mapping(target = "updUserId", source = "modifiedBy")
	@Mapping(target = "updDtm", source = "modifiedTs")
	ca.bc.gov.open.jag.tco.oracledataapi.ords.tco.api.model.AuditLogEntry convertToTcoAuditLogEntry(FileHistory fileHistory);
}
