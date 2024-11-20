import { Injectable } from '@angular/core';
import { ActivatedRoute, NavigationEnd, Router } from '@angular/router';
import { BehaviorSubject, filter } from 'rxjs';
import { Breadcrumb } from 'src/app/shared/types/breadcrumb';

@Injectable()
export class BreadcrumbService {
  private breadcrumbsSubject = new BehaviorSubject<Breadcrumb[]>([]);
  breadcrumbs$ = this.breadcrumbsSubject.asObservable();

  constructor(private router: Router, private route: ActivatedRoute) {
    this.router.events
      .pipe(filter((event) => event instanceof NavigationEnd))
      .subscribe(() => {
        console.log('next navigation');

        this.breadcrumbsSubject.next(this.buildBreadcrumbs(this.route.root));
      });
  }

  private buildBreadcrumbs(
    route: ActivatedRoute,
    breadcrumbs: Breadcrumb[] = [],
    parentBreadcrumb: Breadcrumb | undefined = undefined
  ): Breadcrumb[] {
    const children: ActivatedRoute[] = route.children;
    if (children.length === 0) {
      return [];
    }

    for (const child of children) {
      const breadcrumbData = child.snapshot.data['breadcrumb'];
      const dataSource = child.snapshot.data['dataSource'];

      if (!parentBreadcrumb) {
        const rootData: Breadcrumb | null = child.snapshot.data['root'];
        if (!rootData) throw Error('root breadcrumb not found');

        breadcrumbs.push(rootData);
        parentBreadcrumb = rootData;
      }

      if (breadcrumbData) {
        const childBreadcrumb = this.modifyBreadcrumb(
          breadcrumbData,
          parentBreadcrumb
        );

        breadcrumbs.push(childBreadcrumb);
        parentBreadcrumb = childBreadcrumb;
      }

      if (dataSource && dataSource['breadcrumb']) {
        const childBreadcrumb = this.modifyBreadcrumb(
          dataSource['breadcrumb'],
          parentBreadcrumb
        );

        breadcrumbs.push(childBreadcrumb);
        parentBreadcrumb = childBreadcrumb;
      }

      this.buildBreadcrumbs(child, breadcrumbs, parentBreadcrumb);
    }

    return breadcrumbs;
  }

  private modifyBreadcrumb(
    childBreadcrumb: Breadcrumb,
    parentBreadcrumb: Breadcrumb
  ): Breadcrumb {
    return {
      ...childBreadcrumb,
      url: `${parentBreadcrumb?.url}/${childBreadcrumb.url}`,
    };
  }
}
