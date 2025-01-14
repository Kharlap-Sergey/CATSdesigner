import { Component, OnInit, ViewChild } from '@angular/core';
import { MatDialog, MatSort, MatPaginator, MatTableDataSource } from '@angular/material';
import { AddGroupComponent } from '../modal/add-group/add-group.component';
import { GroupService } from 'src/app/service/group.service';
import { DeleteItemComponent } from '../modal/delete-person/delete-person.component';
import { Group } from 'src/app/model/group';
import { ListOfStudentsComponent } from '../modal/list-of-students/list-of-students.component';
import {MessageComponent} from '../../../component/message/message.component';
import { AppToastrService } from 'src/app/service/toastr.service';
import { SubjectListComponent } from '../modal/subject-list/subject-list.component';

@Component({
  selector: 'app-group',
  templateUrl: './group.component.html',
  styleUrls: ['./group.component.css']
})
export class GroupComponent implements OnInit {

  dataGroup  = new Group();
  displayedColumns: string[] = ['number',  'Name', 'StartYear', 'GraduationYear', 'studentsCount','subjectsCount', 's'];
  dataSource = new MatTableDataSource<object>();
  @ViewChild(MatPaginator, { static: true }) paginator: MatPaginator;
  @ViewChild(MatSort, { static: true }) sort: MatSort;
  isLoad = false;

  constructor(private groupService: GroupService, private dialog: MatDialog, private toastr: AppToastrService) { }

  ngOnInit() {
    this.dataSource.sort = this.sort;
    this.dataSource.paginator = this.paginator;
    this.loadGroup();
  }

  openListOfSubject(group) {
    const dialogRef = this.dialog.open(SubjectListComponent, {
      data: group
    });
    dialogRef.afterClosed();
  }


  loadGroup() {
    this.groupService.getGroups().subscribe(items => {
      this.dataSource.data = items.sort((a,b) => this.sortFunc(a, b));
      this.isLoad = true;
    });
  }

  sortFunc(a, b) { 
    if(a.Name < b.Name){
      return -1;
    }

    else if(a.Name > b.Name){
      return 1;
    }
    
    return 0;
 } 

  saveGroup(group: Group) {
    this.groupService.addGroup(group).subscribe(() => {
      this.loadGroup();
      this.dataGroup = new Group();
      this.toastr.addSuccessFlashMessage("Группа успешна сохранена!");
    }, err => {
      if (err.status === 500) {
        this.loadGroup();
        this.toastr.addSuccessFlashMessage("Группа успешна сохранена!");
      } else {
        this.toastr.addErrorFlashMessage('Произошла ошибка при сохранении. Повторите попытку');
      }
    });
  }

  addGroup(group) {
    const dialogRef = this.dialog.open( AddGroupComponent, {
       data: group
    });
    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.saveGroup(result.data);
      }
    });
  }

  deleteGroup(id, subCount, studCount) {
    const dialogRef = this.dialog.open(DeleteItemComponent);
    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.removeGroup(id, subCount, studCount);
      }
    });
  }

  removeGroup(id, subCount, studCount) {
    this.groupService.deleteGroup(id).subscribe(() => {
      this.loadGroup();
      this.toastr.addSuccessFlashMessage("Группа удалена!");
    }, 
    err => {
        var msg = 'Произошла ошибка!\n';
        if (studCount != 0) {
          msg = msg + 'Нельзя удалить группу со студентами!\n'
        } 

        if (subCount != 0) {
          msg = msg + 'Нельзя удалить группу за которой закреплены предметы!\n'
        }

        this.toastr.addErrorFlashMessage(msg);

        this.loadGroup();
    });
  }

  editGroup(group: Group) {
    const dialogRef = this.dialog.open(AddGroupComponent, {
        data: group
    });
    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.saveGroup(result.data);
      }
    });
  }

  async openListOfStudents(group) {
    const dialogRef = this.dialog.open(ListOfStudentsComponent, 
      {data: group},
    );
    dialogRef.afterClosed();
  }

  applyFilter(filterValue: string) {
    this.dataSource.filter = filterValue.trim().toLowerCase();
  }

}
