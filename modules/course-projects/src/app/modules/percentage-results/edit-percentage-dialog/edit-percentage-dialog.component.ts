import {Component, Inject} from '@angular/core';
import {MAT_DIALOG_DATA, MatDialogRef} from '@angular/material';
import {FormControl, Validators} from '@angular/forms';

interface DialogData {
  mark: number;
  min: number;
  max: number;
  regex: string;
  errorMsg: string;
  label: string;
  symbol: string;
  notEmpty: boolean;
}

@Component({
  selector: 'app-edit-percentage-dialog',
  templateUrl: './edit-percentage-dialog.component.html',
  styleUrls: ['./edit-percentage-dialog.component.less']
})
export class EditPercentageDialogComponent {

  private percentageControl: FormControl = new FormControl(this.data.mark);

  constructor(public dialogRef: MatDialogRef<EditPercentageDialogComponent>,
              @Inject(MAT_DIALOG_DATA) public data: DialogData) {
    const validators = [];
    if (this.data.min != null) { validators.push(Validators.min(this.data.min)); }
    if (this.data.max != null) { validators.push(Validators.max(this.data.max)); }
    if (this.data.regex != null) { validators.push(Validators.pattern(this.data.regex)); }
    if (this.data.notEmpty) { validators.push(Validators.required); }
    this.percentageControl.setValidators(validators);
  }

  onCancelClick(): void {
    this.dialogRef.close();
  }

}