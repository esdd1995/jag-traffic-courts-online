package ca.bc.gov.open.jag.tco.oracledataapi.repository.impl.ords;

import java.text.SimpleDateFormat;
import java.util.Date;
import java.util.List;
import java.util.NoSuchElementException;
import java.util.Optional;

import javax.ws.rs.InternalServerErrorException;

import org.hibernate.cfg.NotYetImplementedException;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Qualifier;
import org.springframework.boot.autoconfigure.condition.ConditionalOnProperty;
import org.springframework.stereotype.Repository;

import ca.bc.gov.open.jag.tco.oracledataapi.api.ViolationTicketApi;
import ca.bc.gov.open.jag.tco.oracledataapi.api.handler.ApiException;
import ca.bc.gov.open.jag.tco.oracledataapi.api.model.ResponseResult;
import ca.bc.gov.open.jag.tco.oracledataapi.api.model.ViolationTicket;
import ca.bc.gov.open.jag.tco.oracledataapi.mapper.DisputeMapper;
import ca.bc.gov.open.jag.tco.oracledataapi.model.Dispute;
import ca.bc.gov.open.jag.tco.oracledataapi.model.DisputeStatus;
import ca.bc.gov.open.jag.tco.oracledataapi.repository.DisputeRepository;

@ConditionalOnProperty(name = "ords.enabled", havingValue = "true", matchIfMissing = false)
@Qualifier("disputeRepository")
@Repository
public class DisputeRepositoryImpl implements DisputeRepository {

	Logger logger = LoggerFactory.getLogger(DisputeRepositoryImpl.class);

	// Delegate, OpenAPI generated client
	private final ViolationTicketApi violationTicketApi;

	public DisputeRepositoryImpl(ViolationTicketApi violationTicketApi) {
		this.violationTicketApi = violationTicketApi;
	}

	@Override
	public Iterable<Dispute> findByCreatedTsBefore(Date olderThan) {
		throw new NotYetImplementedException();
	}

	@Override
	public Iterable<Dispute> findByStatusNot(DisputeStatus excludeStatus) {
		throw new NotYetImplementedException();
	}

	@Override
	public Iterable<Dispute> findByStatusNotAndCreatedTsBefore(DisputeStatus excludeStatus, Date olderThan) {
		throw new NotYetImplementedException();
	}

	@Override
	public Iterable<Dispute> findByUserAssignedTsBefore(Date olderThan) {
		throw new NotYetImplementedException();
	}

	@Override
	public List<Dispute> findByEmailVerificationToken(String emailVerificationToken) {
		throw new NotYetImplementedException();
	}

	@Override
	public void deleteAll() {
		throw new NotYetImplementedException();
	}

	@Override
	public void deleteById(Long disputeId) {
		if (disputeId == null) {
			throw new IllegalArgumentException("DisputeId is null.");
		}

		// Propagate any ApiException to caller
		ResponseResult result = violationTicketApi.v1DeleteViolationTicketDelete(disputeId);

		if (result == null) {
			throw new InternalServerErrorException("Invalid ResponseResult object");
		}
		else if (result.getException() != null) {
			// Known error if no data found
			if ("0".equals(result.getStatus()) && result.getException().startsWith("ORA-01403")) {
				throw new NoSuchElementException(result.getException());
			}
			// Unknown error
			else {
				throw new InternalServerErrorException(result.getException());
			}
		}
		else if (!"1".equals(result.getStatus())) {
			throw new InternalServerErrorException("Dispute deletion status is not 1 (success)");
		}
	}

	@Override
	public Iterable<Dispute> findAll() {
		throw new NotYetImplementedException();
	}

	@Override
	public Optional<Dispute> findById(Long id) {
		if (id == null) {
			throw new IllegalArgumentException("Dispute ID is null.");
		}
		try {
			ViolationTicket violationTicket = violationTicketApi.v1ViolationTicketGet(null, id);
			if (violationTicket == null || violationTicket.getViolationTicketId() == null) {
				return Optional.empty();
			}
			else {
				logger.debug("Successfully returned the violation ticket from ORDS with dispute id {}", id);
				Dispute dispute = DisputeMapper.INSTANCE.convertViolationTicketDtoToDispute(violationTicket);
				return Optional.ofNullable(dispute);
			}
		} catch (ApiException e) {
			// FIXME: this error should be propagated up so the caller (controller) can respond with a 500 InternalServerException
			logger.error("ERROR retrieving Dispute from ORDS with dispute id {}", id, e);
			return Optional.empty();
		}
	}

	@Override
	public Dispute save(Dispute dispute) {
		throw new NotYetImplementedException();
	}

	@Override
	public Dispute saveAndFlush(Dispute entity) {
		throw new NotYetImplementedException();
	}

	@Override
	public void unassignDisputes(Date olderThan) {
		SimpleDateFormat simpleDateFormat = new SimpleDateFormat("yyyy-MM-dd HH:mm:ss");
		String dateStr = simpleDateFormat.format(olderThan);

		// Propagate any ApiException to caller
		ResponseResult result = violationTicketApi.v1UnassignViolationTicketPost(dateStr);

		if (result == null) {
			// unknown if ORDS could unassign or not, missing response object.
			throw new InternalServerErrorException("Invalid ResponseResult object");
		}
		else if (result.getException() != null) {
			// ORDS could not unassign, error message in the response object.
			throw new InternalServerErrorException(result.getException());
		}
		else if (!"1".equals(result.getStatus())) {
			// ORDS could not unassign, error message missing in the response object.
			throw new InternalServerErrorException("Dispute unassign is not 1 (success)");
		}

	}

}
