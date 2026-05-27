export interface NavItem {
  labelKey: string;
  route: string;
  icon: string;
  permission?: string | string[];
}

export interface NavGroup {
  labelKey: string;
  items: NavItem[];
}

export const NAV_GROUPS: NavGroup[] = [
  {
    labelKey: 'nav.groups.main',
    items: [
      { labelKey: 'nav.dashboard', route: '/dashboard', icon: 'bi-speedometer2' },
    ],
  },
  {
    labelKey: 'nav.groups.institution',
    items: [
      {
        labelKey: 'nav.academicYears',
        route: '/institution/academic-years',
        icon: 'bi-calendar3',
        permission: 'institution.academic-years.manage',
      },
      {
        labelKey: 'nav.grades',
        route: '/institution/grades',
        icon: 'bi-mortarboard',
        permission: 'institution.classes.manage',
      },
      {
        labelKey: 'nav.staff',
        route: '/institution/staff',
        icon: 'bi-people',
        permission: 'institution.staff.manage',
      },
      {
        labelKey: 'nav.customFields',
        route: '/institution/custom-fields',
        icon: 'bi-sliders',
        permission: 'institution.staff.fields.manage',
      },
    ],
  },
  {
    labelKey: 'nav.groups.settings',
    items: [
      {
        labelKey: 'nav.roles',
        route: '/settings/roles',
        icon: 'bi-shield-lock',
        permission: 'roles.role.read',
      },
      {
        labelKey: 'nav.auditLogs',
        route: '/settings/audit-logs',
        icon: 'bi-clock-history',
        permission: 'audit.logs.read',
      },
    ],
  },
];
