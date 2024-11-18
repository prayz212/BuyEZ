import {
  AfterContentInit,
  Component,
  ContentChildren,
  Input,
  QueryList,
} from '@angular/core';
import { HorizontalTabsDataSource } from '../../types/horizontal-tabs-data-source';
import { TabContentComponent } from './tab-content/tab-content.component';

@Component({
  selector: 'buyez-horizontal-tabs',
  templateUrl: './horizontal-tabs.component.html',
})
export class TabsComponent implements AfterContentInit {
  @Input() tabs!: HorizontalTabsDataSource[];

  @ContentChildren(TabContentComponent)
  tabContents!: QueryList<TabContentComponent>;

  ngAfterContentInit(): void {
    if (this.tabs.every((tab) => !tab.isActive)) {
      this.tabs[0].isActive = true;
    }

    this.updateTabContent();
  }

  handleTabChanged(tabKey: string) {
    console.log(`Tab selected: ${tabKey}tabKey`);

    this.tabs = this.tabs.map((tab) => ({
      ...tab,
      isActive: tab.key === tabKey ? true : false,
    }));

    this.updateTabContent();
  }

  private updateTabContent(): void {
    if (this.tabContents) {
      this.tabContents.forEach((tabContent, index) => {
        tabContent.isActive = this.tabs[index].isActive;
      });
    }
  }
}
