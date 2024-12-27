import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import {
  BreadcrumbComponent,
  OutlineButtonComponent,
  PaginationComponent,
  SelectComponent,
  SolidButtonComponent,
  TabContentComponent,
  TabsComponent,
} from './components';
import { TextPipe } from './pipes';

@NgModule({
  declarations: [
    /* Components */
    BreadcrumbComponent,
    TabsComponent,
    TabContentComponent,
    OutlineButtonComponent,
    PaginationComponent,
    SelectComponent,
    SolidButtonComponent,

    /* Pipes */
    TextPipe,
  ],
  imports: [RouterModule, CommonModule],
  exports: [
		/* Components */
    BreadcrumbComponent,
    TabsComponent,
    TabContentComponent,
    OutlineButtonComponent,
    PaginationComponent,
    SelectComponent,
    SolidButtonComponent,

    /* Pipes */
    TextPipe,

		/* Other modules */
		RouterModule,
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
  ],
})
export class SharedModule {}
