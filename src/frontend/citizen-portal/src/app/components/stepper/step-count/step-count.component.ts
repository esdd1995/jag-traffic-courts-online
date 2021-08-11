import {
  Component,
  EventEmitter,
  Input,
  OnInit,
  Output,
  ViewChild,
} from '@angular/core';
import { FormBuilder, FormControl, FormGroup } from '@angular/forms';
import { MatChip, MatChipList } from '@angular/material/chips';
import { MatStepper } from '@angular/material/stepper';
import { ActivatedRoute, Router } from '@angular/router';
import { FormUtilsService } from '@core/services/form-utils.service';
import { LoggerService } from '@core/services/logger.service';
import { UtilsService } from '@core/services/utils.service';
import { TranslateService } from '@ngx-translate/core';
import { BaseDisputeFormPage } from 'app/components/classes/BaseDisputeFormPage';
import { DisputeFormStateService } from 'app/services/dispute-form-state.service';
import { DisputeResourceService } from 'app/services/dispute-resource.service';
import { DisputeService } from 'app/services/dispute.service';

@Component({
  selector: 'app-step-count',
  templateUrl: './step-count.component.html',
  styleUrls: ['./step-count.component.scss'],
})
export class StepCountComponent extends BaseDisputeFormPage implements OnInit {
  @Input() public stepper: MatStepper;
  @Input() public stepControl: FormGroup;
  @Output() public stepSave: EventEmitter<MatStepper> = new EventEmitter();
  @Output() public stepCancel: EventEmitter<MatStepper> = new EventEmitter();

  @ViewChild(MatChipList) chipList!: MatChipList;

  public defaultLanguage: string;
  public previousButtonIcon = 'keyboard_arrow_left';
  public previousButtonKey = 'stepper.back';
  public saveButtonKey = 'stepper.next';
  public choice = 0;

  constructor(
    protected route: ActivatedRoute,
    protected router: Router,
    protected formBuilder: FormBuilder,
    protected disputeService: DisputeService,
    protected disputeResource: DisputeResourceService,
    protected disputeFormStateService: DisputeFormStateService,
    private formUtilsService: FormUtilsService,
    private utilsService: UtilsService,
    private logger: LoggerService,
    private translateService: TranslateService
  ) {
    super(
      route,
      router,
      formBuilder,
      disputeService,
      disputeResource,
      disputeFormStateService
    );
  }

  public offenceAgreementStatusOptions = [
    {
      value: 'NOTHING',
      desc: 'I do not wish to take any action on this count at this time.',
    },
    {
      value: 'PAY',
      desc: 'I agree I committed this offence and I would like to pay for this count.',
    },
    {
      value: 'REDUCTION',
      desc: 'I agree I committed this offence and I would like to request a fine reduction and/or more time to pay for this count.',
    },
    {
      value: 'DISPUTE',
      desc: 'I do not agree that I committed this offence and I would like to dispute this count.',
    },
  ];

  public toggleSelection(chip: MatChip) {
    // chip.toggleSelected();
    chip.select();
    this.logger.log('chip', chip);
  }

  public ngOnInit() {
    this.defaultLanguage = this.translateService.getDefaultLang();
    this.form = this.stepControl;
    this.patchForm();
  }

  public onSubmit(): void {
    if (this.formUtilsService.checkValidity(this.form)) {
      this.stepSave.emit(this.stepper);
    } else {
      this.utilsService.scrollToErrorSection();
    }
  }

  public onBack() {
    this.stepper.previous();
  }

  public get offenceAgreementStatus(): FormControl {
    return this.form.get('offenceAgreementStatus') as FormControl;
  }

  public get reductionAppearInCourt(): FormControl {
    return this.form.get('reductionAppearInCourt') as FormControl;
  }

  public get requestReduction(): FormControl {
    return this.form.get('requestReduction') as FormControl;
  }

  public get requestMoreTime(): FormControl {
    return this.form.get('requestMoreTime') as FormControl;
  }

  public get reductionReason(): FormControl {
    return this.form.get('reductionReason') as FormControl;
  }

  public get moreTimeReason(): FormControl {
    return this.form.get('moreTimeReason') as FormControl;
  }

  public get offenceDescription(): FormControl {
    return this.form.get('offenceDescription') as FormControl;
  }

  public get offenceNumber(): FormControl {
    return this.form.get('offenceNumber') as FormControl;
  }

  public get ticketedAmount(): FormControl {
    return this.form.get('ticketedAmount') as FormControl;
  }

  public get amountDue(): FormControl {
    return this.form.get('amountDue') as FormControl;
  }

  public get discountAmount(): FormControl {
    return this.form.get('discountAmount') as FormControl;
  }

  public get discountDueDate(): FormControl {
    return this.form.get('discountDueDate') as FormControl;
  }
}
