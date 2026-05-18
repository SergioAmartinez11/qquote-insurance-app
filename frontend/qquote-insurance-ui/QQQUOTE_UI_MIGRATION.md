# Guía de Migración de UI — AuraImmex → QQuote

Este documento describe todos los archivos, componentes, estilos y configuraciones necesarios para replicar el **shell visual** de AuraImmex (login + layout con menú lateral) en el proyecto QQuote. El dominio de negocio (rutas, módulos, servicios) es distinto; solo se migra el sistema de UI.

---

## 1. Stack de dependencias

```json
{
  "@angular/core": "^21",
  "@primeuix/themes": "^2.0.0",
  "primeng": "^21.0.2",
  "primeicons": "^7.0.0",
  "tailwindcss": "^4.1.11",
  "tailwindcss-primeui": "^0.6.1",
  "@tailwindcss/postcss": "^4.1.11",
  "chart.js": "4.4.2"
}
```

Instalar con:

```bash
npm install primeng @primeuix/themes primeicons tailwindcss tailwindcss-primeui @tailwindcss/postcss
```

---

## 2. Configuración global de la app

### `src/app.config.ts`

```typescript
import { provideHttpClient, withFetch } from '@angular/common/http';
import { ApplicationConfig, provideZonelessChangeDetection } from '@angular/core';
import { provideRouter, withEnabledBlockingInitialNavigation, withInMemoryScrolling } from '@angular/router';
import Aura from '@primeuix/themes/aura';
import { providePrimeNG } from 'primeng/config';
import { appRoutes } from './app.routes';

export const appConfig: ApplicationConfig = {
    providers: [
        provideRouter(
            appRoutes,
            withInMemoryScrolling({ anchorScrolling: 'enabled', scrollPositionRestoration: 'enabled' }),
            withEnabledBlockingInitialNavigation()
        ),
        provideHttpClient(withFetch()),
        provideZonelessChangeDetection(),          // Sin Zone.js — todo via Signals
        providePrimeNG({
            theme: {
                preset: Aura,
                options: { darkModeSelector: '.app-dark' }   // clase en <html>
            }
        })
    ]
};
```

**Puntos clave:**
- `provideZonelessChangeDetection()` — obligatorio. Sin esto los Signals no disparan correctamente.
- El color primario se sobreescribe en el tema (ver sección 8). El preset `Aura` define la forma de los componentes.
- `darkModeSelector: '.app-dark'` — el dark mode se activa añadiendo esta clase a `<html>`.

### `src/app.component.ts`

```typescript
import { Component } from '@angular/core';
import { RouterModule } from '@angular/router';

@Component({
    selector: 'app-root',
    standalone: true,
    imports: [RouterModule],
    template: `<router-outlet></router-outlet>`
})
export class AppComponent {}
```

### `src/app.routes.ts` (estructura base)

```typescript
import { Routes } from '@angular/router';
import { AppLayout } from './app/layout/component/app.layout';

export const appRoutes: Routes = [
    {
        path: '',
        component: AppLayout,          // Shell con topbar + sidebar
        children: [
            // Aquí van las rutas de QQuote (dashboard, módulos, etc.)
        ]
    },
    { path: 'auth', loadChildren: () => import('./app/pages/auth/auth.routes') },
    { path: '**', redirectTo: '/notfound' }
];
```

---

## 3. Layout Service (estado global del shell)

### `src/app/layout/service/layout.service.ts`

```typescript
import { Injectable, effect, signal, computed } from '@angular/core';

export interface LayoutConfig {
    preset: string;
    primary: string;
    surface: string | undefined | null;
    darkTheme: boolean;
    menuMode: string;    // 'static' | 'overlay'
}

interface LayoutState {
    staticMenuDesktopInactive: boolean;
    overlayMenuActive: boolean;
    configSidebarVisible: boolean;
    mobileMenuActive: boolean;
    menuHoverActive: boolean;
    activePath: string | null;
}

@Injectable({ providedIn: 'root' })
export class LayoutService {
    layoutConfig = signal<LayoutConfig>({
        preset: 'Aura',
        primary: 'emerald',   // color primario — cambiar aquí para QQuote
        surface: null,
        darkTheme: false,
        menuMode: 'static'
    });

    layoutState = signal<LayoutState>({
        staticMenuDesktopInactive: false,
        overlayMenuActive: false,
        configSidebarVisible: false,
        mobileMenuActive: false,
        menuHoverActive: false,
        activePath: null
    });

    theme           = computed(() => this.layoutConfig().darkTheme ? 'light' : 'dark');
    isSidebarActive = computed(() => this.layoutState().overlayMenuActive || this.layoutState().mobileMenuActive);
    isDarkTheme     = computed(() => this.layoutConfig().darkTheme);
    getPrimary      = computed(() => this.layoutConfig().primary);
    getSurface      = computed(() => this.layoutConfig().surface);
    isOverlay       = computed(() => this.layoutConfig().menuMode === 'overlay');
    transitionComplete = signal<boolean>(false);

    private initialized = false;

    constructor() {
        effect(() => {
            const config = this.layoutConfig();
            if (!this.initialized || !config) { this.initialized = true; return; }
            this.handleDarkModeTransition(config);
        });
    }

    private handleDarkModeTransition(config: LayoutConfig): void {
        if ('startViewTransition' in document) {
            document.startViewTransition(() => this.toggleDarkMode(config));
        } else {
            this.toggleDarkMode(config);
        }
    }

    toggleDarkMode(config?: LayoutConfig): void {
        const _config = config || this.layoutConfig();
        if (_config.darkTheme) {
            document.documentElement.classList.add('app-dark');
        } else {
            document.documentElement.classList.remove('app-dark');
        }
    }

    onMenuToggle() {
        if (this.isOverlay()) {
            this.layoutState.update(prev => ({ ...prev, overlayMenuActive: !prev.overlayMenuActive }));
        }
        if (this.isDesktop()) {
            this.layoutState.update(prev => ({ ...prev, staticMenuDesktopInactive: !prev.staticMenuDesktopInactive }));
        } else {
            this.layoutState.update(prev => ({ ...prev, mobileMenuActive: !prev.mobileMenuActive }));
        }
    }

    isDesktop() { return window.innerWidth > 991; }
    isMobile()  { return !this.isDesktop(); }
}
```

---

## 4. Componentes del shell (layout)

### Árbol de archivos

```
src/app/layout/
├── service/
│   └── layout.service.ts          ← estado global (sección 3)
└── component/
    ├── app.layout.ts              ← wrapper principal
    ├── app.topbar.ts              ← barra superior
    ├── app.sidebar.ts             ← barra lateral (contenedor)
    ├── app.menu.ts                ← árbol de navegación
    ├── app.menuitem.ts            ← ítem recursivo del menú
    ├── app.footer.ts              ← pie de página
    ├── app.configurator.ts        ← panel flotante de tema (opcional)
    └── app.floatingconfigurator.ts ← botones dark mode + paleta (login)
```

---

### `app.layout.ts`

```typescript
import { Component, computed, effect, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { AppTopbar } from './app.topbar';
import { AppSidebar } from './app.sidebar';
import { AppFooter } from './app.footer';
import { LayoutService } from '../service/layout.service';

@Component({
    selector: 'app-layout',
    standalone: true,
    imports: [CommonModule, AppTopbar, AppSidebar, RouterModule, AppFooter],
    template: `
        <div class="layout-wrapper" [ngClass]="containerClass()">
            <app-topbar></app-topbar>
            <app-sidebar></app-sidebar>
            <div class="layout-main-container">
                <div class="layout-main">
                    <router-outlet></router-outlet>
                </div>
                <app-footer></app-footer>
            </div>
            <div class="layout-mask"></div>
        </div>
    `
})
export class AppLayout {
    layoutService = inject(LayoutService);

    constructor() {
        effect(() => {
            const state = this.layoutService.layoutState();
            document.body.classList.toggle('blocked-scroll', state.mobileMenuActive);
        });
    }

    containerClass = computed(() => {
        const config = this.layoutService.layoutConfig();
        const state  = this.layoutService.layoutState();
        return {
            'layout-overlay':          config.menuMode === 'overlay',
            'layout-static':           config.menuMode === 'static',
            'layout-static-inactive':  state.staticMenuDesktopInactive && config.menuMode === 'static',
            'layout-overlay-active':   state.overlayMenuActive,
            'layout-mobile-active':    state.mobileMenuActive
        };
    });
}
```

---

### `app.topbar.ts`

Componente de la barra superior con: botón hamburguesa, logo, breadcrumb, dark mode toggle, paleta de colores y menú de usuario.

```typescript
import { Component, inject, signal } from '@angular/core';
import { MenuItem } from 'primeng/api';
import { NavigationEnd, Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { StyleClassModule } from 'primeng/styleclass';
import { BreadcrumbModule } from 'primeng/breadcrumb';
import { filter } from 'rxjs/operators';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { AppConfigurator } from './app.configurator';
import { LayoutService } from '../service/layout.service';
import { Menubar } from 'primeng/menubar';

@Component({
    selector: 'app-topbar',
    standalone: true,
    imports: [RouterModule, CommonModule, StyleClassModule, AppConfigurator, BreadcrumbModule, Menubar],
    styles: [`
        :host ::ng-deep .topbar-user-menubar {
            background: transparent;
            border: none;
            padding: 0;
            .p-menubar-root-list { gap: 0.35rem; }
            .p-menubar-item-link { padding: 0; background: transparent; }
            .p-menubar-item-link:hover,
            .p-menubar-item-link:focus { background: transparent; }
            .p-menubar-submenu { top: calc(100% + 0.5rem); right: 0; left: auto; min-width: 12rem; }
        }
    `],
    template: `
        <div class="layout-topbar">
            <div class="layout-topbar-logo-container">
                <button class="layout-menu-button layout-topbar-action" (click)="layoutService.onMenuToggle()">
                    <i class="pi pi-bars"></i>
                </button>
                <!-- REEMPLAZAR: logo de QQuote aquí -->
                <a class="layout-topbar-logo" routerLink="/">
                    <span>QQuote</span>
                </a>
            </div>

            <p-breadcrumb [model]="breadcrumbItems()" [home]="breadcrumbHome"></p-breadcrumb>

            <div class="layout-topbar-actions">
                <div class="layout-config-menu">
                    <button type="button" class="layout-topbar-action" (click)="toggleDarkMode()">
                        <i [class]="layoutService.isDarkTheme() ? 'pi pi-sun' : 'pi pi-moon'"></i>
                    </button>
                    <div class="relative">
                        <button class="layout-topbar-action layout-topbar-action-highlight"
                                pStyleClass="@next"
                                enterFromClass="hidden" enterActiveClass="animate-scalein"
                                leaveToClass="hidden"  leaveActiveClass="animate-fadeout"
                                [hideOnOutsideClick]="true">
                            <i class="pi pi-palette"></i>
                        </button>
                        <app-configurator />
                    </div>
                    <p-menubar [model]="userProfileMenu" styleClass="topbar-user-menubar">
                        <ng-template #item let-item let-root="root">
                            @if (root) {
                                <a class="layout-topbar-action" role="button">
                                    <i [class]="item.icon"></i>
                                </a>
                            } @else {
                                <a class="p-menubar-item-link flex items-center gap-2 px-3 py-2 cursor-pointer">
                                    <i [class]="item.icon"></i>
                                    <span>{{ item.label }}</span>
                                </a>
                            }
                        </ng-template>
                    </p-menubar>
                </div>
            </div>
        </div>
    `
})
export class AppTopbar {
    layoutService   = inject(LayoutService);
    private router  = inject(Router);

    breadcrumbHome: MenuItem = { icon: 'pi pi-home', routerLink: '/' };
    breadcrumbItems = signal<MenuItem[]>([]);

    // REEMPLAZAR: menú de usuario para QQuote
    userProfileMenu: MenuItem[] = [
        {
            icon: 'pi pi-user',
            items: [
                { label: 'Settings', icon: 'pi pi-cog',      command: () => this.router.navigate(['/settings']) },
                { separator: true },
                { label: 'Logout',   icon: 'pi pi-sign-out', command: () => this.router.navigate(['/auth/login']) }
            ]
        }
    ];

    private labelMap: Record<string, string> = {};   // personalizar labels del breadcrumb

    constructor() {
        this.buildBreadcrumb(this.router.url);
        this.router.events
            .pipe(filter((e): e is NavigationEnd => e instanceof NavigationEnd), takeUntilDestroyed())
            .subscribe(e => this.buildBreadcrumb(e.urlAfterRedirects));
    }

    private buildBreadcrumb(url: string): void {
        const segments = url.split('?')[0].split('#')[0].split('/').filter(Boolean);
        let path = '';
        const items: MenuItem[] = segments.map((seg, idx) => {
            path += '/' + seg;
            const isLast = idx === segments.length - 1;
            const label = this.labelMap[seg] ?? (decodeURIComponent(seg).replace(/[-_]/g, ' '));
            return { label: label.charAt(0).toUpperCase() + label.slice(1), ...(isLast ? {} : { routerLink: path }) };
        });
        this.breadcrumbItems.set(items);
    }

    toggleDarkMode() {
        this.layoutService.layoutConfig.update(s => ({ ...s, darkTheme: !s.darkTheme }));
    }
}
```

---

### `app.sidebar.ts`

```typescript
import { Component, effect, ElementRef, inject, OnDestroy, OnInit } from '@angular/core';
import { NavigationEnd, Router, RouterModule } from '@angular/router';
import { filter, Subject, takeUntil } from 'rxjs';
import { AppMenu } from './app.menu';
import { LayoutService } from '../service/layout.service';

@Component({
    selector: 'app-sidebar',
    standalone: true,
    imports: [AppMenu, RouterModule],
    template: `
        <div class="layout-sidebar">
            <app-menu></app-menu>
        </div>
    `
})
export class AppSidebar implements OnInit, OnDestroy {
    layoutService = inject(LayoutService);
    router        = inject(Router);
    el            = inject(ElementRef);

    private outsideClickListener: ((e: MouseEvent) => void) | null = null;
    private destroy$ = new Subject<void>();

    constructor() {
        effect(() => {
            const state = this.layoutService.layoutState();
            const active = this.layoutService.isDesktop() ? state.overlayMenuActive : state.mobileMenuActive;
            active ? this.bindOutsideClickListener() : this.unbindOutsideClickListener();
        });
    }

    ngOnInit() {
        this.router.events
            .pipe(filter(e => e instanceof NavigationEnd), takeUntil(this.destroy$))
            .subscribe((e: any) => this.onRouteChange(e.urlAfterRedirects));
        this.onRouteChange(this.router.url);
    }

    ngOnDestroy() {
        this.destroy$.next();
        this.destroy$.complete();
        this.unbindOutsideClickListener();
    }

    private onRouteChange(path: string) {
        this.layoutService.layoutState.update(val => ({
            ...val, activePath: path, overlayMenuActive: false,
            staticMenuMobileActive: false, mobileMenuActive: false, menuHoverActive: false
        }));
    }

    private bindOutsideClickListener() {
        if (!this.outsideClickListener) {
            this.outsideClickListener = (event: MouseEvent) => {
                if (this.isOutsideClicked(event)) {
                    this.layoutService.layoutState.update(val => ({
                        ...val, overlayMenuActive: false, staticMenuMobileActive: false,
                        mobileMenuActive: false, menuHoverActive: false
                    }));
                }
            };
            document.addEventListener('click', this.outsideClickListener);
        }
    }

    private unbindOutsideClickListener() {
        if (this.outsideClickListener) {
            document.removeEventListener('click', this.outsideClickListener);
            this.outsideClickListener = null;
        }
    }

    private isOutsideClicked(event: MouseEvent): boolean {
        const topbarBtn = document.querySelector('.topbar-start > button');
        const sidebar   = this.el.nativeElement;
        return !(sidebar?.contains(event.target as Node) || topbarBtn?.contains(event.target as Node));
    }
}
```

---

### `app.menu.ts`

Reemplazar `model` con la estructura de navegación de QQuote. La estructura del componente no cambia.

```typescript
import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MenuItem } from 'primeng/api';
import { AppMenuitem } from './app.menuitem';

@Component({
    selector: 'app-menu',
    standalone: true,
    imports: [CommonModule, AppMenuitem, RouterModule],
    template: `
        <ul class="layout-menu">
            @for (item of model; track item.label) {
                @if (!item.separator) {
                    <li app-menuitem [item]="item" [root]="true"></li>
                } @else {
                    <li class="menu-separator"></li>
                }
            }
        </ul>
    `
})
export class AppMenu {
    model: MenuItem[] = [];

    ngOnInit() {
        // REEMPLAZAR: definir navegación de QQuote
        this.model = [
            {
                label: 'PRINCIPAL',
                items: [
                    { label: 'Dashboard', icon: 'pi pi-fw pi-home', routerLink: ['/'] }
                ]
            },
            {
                label: 'MÓDULO A',
                items: [
                    { label: 'Sección 1', icon: 'pi pi-fw pi-list',  routerLink: ['/modulo-a/seccion1'] },
                    { label: 'Sección 2', icon: 'pi pi-fw pi-file',  routerLink: ['/modulo-a/seccion2'] }
                ]
            }
        ];
    }
}
```

---

### `app.menuitem.ts`

Componente recursivo — copiar sin modificar.

```typescript
import { Component, computed, inject, input, signal } from '@angular/core';
import { NavigationEnd, Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { RippleModule } from 'primeng/ripple';
import { LayoutService } from '../service/layout.service';
import { filter } from 'rxjs/operators';

@Component({
    selector: '[app-menuitem]',
    standalone: true,
    imports: [CommonModule, RouterModule, RippleModule],
    host: {
        '[class.active-menuitem]':    'isActive()',
        '[class.layout-root-menuitem]': 'root()'
    },
    styles: [`
        .p-submenu-enter {
            animation: p-animate-submenu-expand 450ms cubic-bezier(0.86, 0, 0.07, 1) forwards;
        }
        .p-submenu-leave {
            animation: p-animate-submenu-collapse 450ms cubic-bezier(0.86, 0, 0.07, 1) forwards;
        }
        @keyframes p-animate-submenu-expand {
            from { max-height: 0; overflow: hidden; }
            to   { max-height: 1000px; overflow: visible; }
        }
        @keyframes p-animate-submenu-collapse {
            from { max-height: 1000px; overflow: hidden; }
            to   { max-height: 0; overflow: hidden; }
        }
    `],
    template: `
        @if (root() && isVisible()) {
            <div class="layout-menuitem-root-text">{{ item().label }}</div>
        }
        @if ((!hasRouterLink() || hasChildren()) && isVisible()) {
            <a [attr.href]="item().url" (click)="itemClick($event)" [ngClass]="item().class"
               [attr.target]="item().target" tabindex="0" pRipple>
                <i [ngClass]="item().icon" class="layout-menuitem-icon"></i>
                <span class="layout-menuitem-text">{{ item().label }}</span>
                @if (hasChildren()) { <i class="pi pi-fw pi-angle-down layout-submenu-toggler"></i> }
            </a>
        }
        @if (hasRouterLink() && !hasChildren() && isVisible()) {
            <a (click)="itemClick($event)" [ngClass]="item().class" [routerLink]="item().routerLink"
               routerLinkActive="active-route"
               [routerLinkActiveOptions]="item().routerLinkActiveOptions || { paths: 'exact', queryParams: 'ignored', matrixParams: 'ignored', fragment: 'ignored' }"
               [attr.target]="item().target" tabindex="0" pRipple>
                <i [ngClass]="item().icon" class="layout-menuitem-icon"></i>
                <span class="layout-menuitem-text">{{ item().label }}</span>
            </a>
        }
        @if (hasChildren() && isVisible() && (root() || isActive())) {
            <ul [animate.enter]="initialized() ? 'p-submenu-enter' : null" [animate.leave]="'p-submenu-leave'"
                [class.layout-root-submenulist]="root()">
                @for (child of item().items; track child?.label) {
                    <li app-menuitem [item]="child" [parentPath]="fullPath()" [root]="false"></li>
                }
            </ul>
        }
    `
})
export class AppMenuitem {
    layoutService = inject(LayoutService);
    router        = inject(Router);
    item          = input<any>(null);
    root          = input<boolean>(false);
    parentPath    = input<string | null>(null);

    isVisible    = computed(() => this.item()?.visible !== false);
    hasChildren  = computed(() => !!this.item()?.items?.length);
    hasRouterLink = computed(() => !!this.item()?.routerLink);
    initialized  = signal<boolean>(false);

    fullPath = computed(() => {
        const itemPath = this.item()?.path;
        if (!itemPath) return this.parentPath();
        const parent = this.parentPath();
        return (parent && !itemPath.startsWith(parent)) ? parent + itemPath : itemPath;
    });

    isActive = computed(() => {
        const activePath = this.layoutService.layoutState().activePath;
        return this.item()?.path ? (activePath?.startsWith(this.fullPath() ?? '') ?? false) : false;
    });

    constructor() {
        this.router.events
            .pipe(filter(e => e instanceof NavigationEnd))
            .subscribe(() => { if (this.item()?.routerLink) this.updateActiveStateFromRoute(); });
    }

    ngOnInit()       { if (this.item()?.routerLink) this.updateActiveStateFromRoute(); }
    ngAfterViewInit() { setTimeout(() => this.initialized.set(true)); }

    updateActiveStateFromRoute() {
        const item = this.item();
        if (!item?.routerLink) return;
        const isRouteActive = this.router.isActive(item.routerLink[0], {
            paths: 'exact', queryParams: 'ignored', matrixParams: 'ignored', fragment: 'ignored'
        });
        if (isRouteActive && this.parentPath()) {
            this.layoutService.layoutState.update(val => ({ ...val, activePath: this.parentPath() }));
        }
    }

    itemClick(event: Event) {
        const item = this.item();
        if (item?.disabled) { event.preventDefault(); return; }
        if (item?.command) item.command({ originalEvent: event, item });
        if (this.hasChildren()) {
            this.layoutService.layoutState.update(val => ({
                ...val,
                activePath: this.isActive() ? this.parentPath() : this.fullPath(),
                menuHoverActive: !this.isActive()
            }));
        } else {
            this.layoutService.layoutState.update(val => ({
                ...val, overlayMenuActive: false, staticMenuMobileActive: false,
                mobileMenuActive: false, menuHoverActive: false
            }));
        }
    }
}
```

---

### `app.footer.ts`

```typescript
import { Component } from '@angular/core';

@Component({
    standalone: true,
    selector: 'app-footer',
    template: `
        <div class="layout-footer">
            QQuote &mdash; Copyright 2026 &copy;
        </div>
    `
})
export class AppFooter {}
```

---

### `app.floatingconfigurator.ts` (usado en el login)

```typescript
import { Component, computed, inject, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ButtonModule } from 'primeng/button';
import { StyleClassModule } from 'primeng/styleclass';
import { AppConfigurator } from './app.configurator';
import { LayoutService } from '../service/layout.service';

@Component({
    selector: 'app-floating-configurator',
    standalone: true,
    imports: [CommonModule, ButtonModule, StyleClassModule, AppConfigurator],
    template: `
        <div class="flex gap-4 top-8 right-8" [ngClass]="{'fixed': float()}">
            <p-button type="button" (onClick)="toggleDarkMode()" [rounded]="true"
                      [icon]="isDarkTheme() ? 'pi pi-moon' : 'pi pi-sun'" severity="secondary" />
            <div class="relative">
                <p-button icon="pi pi-palette" pStyleClass="@next"
                          enterFromClass="hidden" enterActiveClass="animate-scalein"
                          leaveToClass="hidden" leaveActiveClass="animate-fadeout"
                          [hideOnOutsideClick]="true" type="button" [rounded]="true" />
                <app-configurator />
            </div>
        </div>
    `
})
export class AppFloatingConfigurator {
    layoutService = inject(LayoutService);
    float         = input<boolean>(true);
    isDarkTheme   = computed(() => this.layoutService.layoutConfig().darkTheme);

    toggleDarkMode() {
        this.layoutService.layoutConfig.update(s => ({ ...s, darkTheme: !s.darkTheme }));
    }
}
```

---

### `app.configurator.ts`

Panel de tema con selección de preset, color primario y superficie. Copiar tal cual de AuraImmex (no se transcribe aquí porque es extenso y no requiere cambios para QQuote). El archivo se encuentra en `src/app/layout/component/app.configurator.ts`.

---

## 5. Página de Login

### `src/app/pages/auth/login/login.ts`

```typescript
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { CheckboxModule } from 'primeng/checkbox';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { RippleModule } from 'primeng/ripple';
import { DividerModule } from 'primeng/divider';
import { AppFloatingConfigurator } from '../../../layout/component/app.floatingconfigurator';

@Component({
    selector: 'app-login',
    standalone: true,
    imports: [ButtonModule, CheckboxModule, InputTextModule, PasswordModule,
              FormsModule, RouterModule, RippleModule, DividerModule, AppFloatingConfigurator],
    templateUrl: './login.component.html'
})
export class Login {
    email: string   = '';
    password: string = '';
    checked: boolean = false;

    signInWithGoogle(): void {
        // implementar OAuth
    }
}
```

### `src/app/pages/auth/login/login.component.html`

```html
<app-floating-configurator />
<div class="bg-surface-50 dark:bg-surface-950 flex items-center justify-center min-h-screen min-w-screen overflow-hidden">
    <div class="flex flex-col items-center justify-center">
        <!-- Tarjeta con gradiente en el borde superior -->
        <div style="border-radius: 56px; padding: 0.3rem; background: linear-gradient(180deg, var(--primary-color) 10%, rgba(33,150,243,0) 30%)">
            <div class="w-full bg-surface-0 dark:bg-surface-900 py-20 px-8 sm:px-20" style="border-radius: 53px">

                <!-- Logo / Heading -->
                <div class="text-center mb-8">
                    <!-- REEMPLAZAR: SVG del logo de QQuote -->
                    <div class="text-surface-900 dark:text-surface-0 text-3xl font-medium mb-4">
                        Bienvenido a QQuote
                    </div>
                    <span class="text-muted-color font-medium">Inicia sesión para continuar</span>
                </div>

                <!-- Formulario -->
                <div>
                    <label for="email1" class="block text-surface-900 dark:text-surface-0 text-xl font-medium mb-2">
                        Email
                    </label>
                    <input pInputText id="email1" type="text" placeholder="Dirección de email"
                           class="w-full md:w-120 mb-8" [(ngModel)]="email" />

                    <label for="password1" class="block text-surface-900 dark:text-surface-0 font-medium text-xl mb-2">
                        Contraseña
                    </label>
                    <p-password id="password1" [(ngModel)]="password" placeholder="Contraseña"
                                [toggleMask]="true" styleClass="mb-4" [fluid]="true" [feedback]="false">
                    </p-password>

                    <div class="flex items-center justify-between mt-2 mb-8 gap-8">
                        <div class="flex items-center">
                            <p-checkbox [(ngModel)]="checked" id="rememberme1" binary class="mr-2"></p-checkbox>
                            <label for="rememberme1">Recuérdame</label>
                        </div>
                        <span class="font-medium no-underline ml-2 text-right cursor-pointer text-primary">
                            ¿Olvidaste tu contraseña?
                        </span>
                    </div>

                    <p-button label="Iniciar Sesión" styleClass="w-full" routerLink="/"></p-button>

                    <p-divider align="center" styleClass="my-6">
                        <span class="text-muted-color font-medium">o</span>
                    </p-divider>

                    <!-- Botón Google OAuth -->
                    <p-button styleClass="w-full" [outlined]="true" severity="secondary" (onClick)="signInWithGoogle()">
                        <ng-template pTemplate="content">
                            <span class="flex items-center justify-center gap-3 w-full">
                                <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 48 48">
                                    <path fill="#FFC107" d="M43.611,20.083H42V20H24v8h11.303c-1.649,4.657-6.08,8-11.303,8c-6.627,0-12-5.373-12-12s5.373-12,12-12c3.059,0,5.842,1.154,7.961,3.039l5.657-5.657C34.046,6.053,29.268,4,24,4C12.955,4,4,12.955,4,24s8.955,20,20,20s20-8.955,20-20C44,22.659,43.862,21.35,43.611,20.083z"/>
                                    <path fill="#FF3D00" d="M6.306,14.691l6.571,4.819C14.655,15.108,18.961,12,24,12c3.059,0,5.842,1.154,7.961,3.039l5.657-5.657C34.046,6.053,29.268,4,24,4C16.318,4,9.656,8.337,6.306,14.691z"/>
                                    <path fill="#4CAF50" d="M24,44c5.166,0,9.86-1.977,13.409-5.192l-6.19-5.238C29.211,35.091,26.715,36,24,36c-5.202,0-9.619-3.317-11.283-7.946l-6.522,5.025C9.505,39.556,16.227,44,24,44z"/>
                                    <path fill="#1976D2" d="M43.611,20.083H42V20H24v8h11.303c-0.792,2.237-2.231,4.166-4.087,5.571l6.19,5.238C36.971,39.205,44,34,44,24C44,22.659,43.862,21.35,43.611,20.083z"/>
                                </svg>
                                <span class="font-medium">Iniciar con Google</span>
                            </span>
                        </ng-template>
                    </p-button>
                </div>

            </div>
        </div>
    </div>
</div>
```

### Diseño visual del login

```
┌──────────────────────────────────────┐  ← fondo bg-surface-50 (claro) / bg-surface-950 (oscuro)
│                                      │
│   ╔══════════════════════════════╗   │  ← borde con gradiente: primary-color → transparente
│   ║  ╔════════════════════════╗  ║   │  ← tarjeta blanca / surface-900 (oscuro), border-radius 53px
│   ║  ║    [LOGO / SVG]        ║  ║   │
│   ║  ║  Bienvenido a QQuote   ║  ║   │
│   ║  ║  Inicia sesión...      ║  ║   │
│   ║  ║                        ║  ║   │
│   ║  ║  Email: [__________]   ║  ║   │  ← pInputText
│   ║  ║  Password: [________]  ║  ║   │  ← p-password [toggleMask]
│   ║  ║  [x] Recuérdame   ¿..? ║  ║   │  ← p-checkbox + link
│   ║  ║  [ Iniciar Sesión ]    ║  ║   │  ← p-button full-width
│   ║  ║  ─────── o ──────────  ║  ║   │  ← p-divider
│   ║  ║  [ G  Iniciar Google ] ║  ║   │  ← p-button outlined secondary
│   ║  ╚════════════════════════╝  ║   │
│   ╚══════════════════════════════╝   │
└──────────────────────────────────────┘

[☀/🌙]  [🎨]  ← app-floating-configurator (fixed top-8 right-8)
```

### Rutas de auth

```typescript
// src/app/pages/auth/auth.routes.ts
import { Routes } from '@angular/router';
import { Login } from './login/login';

export default [
    { path: 'login', component: Login },
    // { path: 'error',  component: Error  },
    // { path: 'access', component: Access },
] as Routes;
```

---

## 6. Estilos SCSS del layout

Crear la estructura de archivos:

```
src/assets/
├── layout/
│   ├── layout.scss              ← punto de entrada
│   ├── variables/
│   │   └── _common.scss         ← mapeo de variables CSS → tokens PrimeNG
│   ├── _core.scss
│   ├── _main.scss
│   ├── _topbar.scss
│   ├── _menu.scss
│   ├── _footer.scss
│   ├── _responsive.scss
│   └── _mixins.scss
├── styles.scss                  ← entry point global
└── tailwind.css                 ← Tailwind v4 entry
```

### `src/assets/layout/layout.scss`

```scss
@use './variables/_common';
@use './_mixins';
@use './_core';
@use './_main';
@use './_topbar';
@use './_menu';
@use './_footer';
@use './_responsive';
```

### `src/assets/layout/variables/_common.scss`

```scss
:root {
    --primary-color:               var(--p-primary-color);
    --primary-contrast-color:      var(--p-primary-contrast-color);
    --text-color:                  var(--p-text-color);
    --text-color-secondary:        var(--p-text-muted-color);
    --surface-border:              var(--p-content-border-color);
    --surface-card:                var(--p-content-background);
    --surface-hover:               var(--p-content-hover-background);
    --surface-overlay:             var(--p-overlay-popover-background);
    --transition-duration:         var(--p-transition-duration);
    --maskbg:                      var(--p-mask-background);
    --content-border-radius:       var(--p-content-border-radius);
    --layout-section-transition-duration: 0.2s;
    --element-transition-duration: var(--p-transition-duration);
    --focus-ring-width:            var(--p-focus-ring-width);
    --focus-ring-style:            var(--p-focus-ring-style);
    --focus-ring-color:            var(--p-focus-ring-color);
    --focus-ring-offset:           var(--p-focus-ring-offset);
    --focus-ring-shadow:           var(--p-focus-ring-shadow);
}
```

### `src/assets/layout/_core.scss`

```scss
html {
    height: 100%;
    font-size: 14px;
    line-height: 1.2;
}

body {
    font-family: 'Lato', sans-serif;
    color: var(--text-color);
    background-color: var(--surface-ground);
    margin: 0;
    padding: 0;
    min-height: 100%;
    -webkit-font-smoothing: antialiased;
}

a { text-decoration: none; }

.layout-wrapper { min-height: 100vh; }
```

### `src/assets/layout/_main.scss`

```scss
.layout-main-container {
    display: flex;
    flex-direction: column;
    min-height: 100vh;
    justify-content: space-between;
    padding: 6rem 2rem 0 2rem;
    transition: margin-left var(--layout-section-transition-duration);
}

.layout-main {
    flex: 1 1 auto;
    padding-bottom: 2rem;
}
```

### `src/assets/layout/_topbar.scss`

```scss
@use 'mixins' as *;

.layout-topbar {
    position: fixed;
    height: 4rem;
    z-index: 997;
    left: 0; top: 0;
    width: 100%;
    padding: 0 2rem;
    background-color: var(--surface-card);
    transition: left var(--layout-section-transition-duration);
    display: flex;
    align-items: center;

    .layout-topbar-logo-container {
        width: 20rem;
        display: flex;
        align-items: center;
    }

    .layout-topbar-logo {
        display: inline-flex;
        align-items: center;
        font-size: 1.5rem;
        border-radius: var(--content-border-radius);
        color: var(--text-color);
        font-weight: 500;
        gap: 0.5rem;
        svg { width: 3rem; }
    }

    .layout-topbar-action {
        display: inline-flex;
        justify-content: center;
        align-items: center;
        border-radius: 50%;
        width: 2.5rem;
        height: 2.5rem;
        color: var(--text-color);
        transition: background-color var(--element-transition-duration);
        cursor: pointer;

        &:hover { background-color: var(--surface-hover); }
        i { font-size: 1.25rem; }

        &.layout-topbar-action-highlight {
            background-color: var(--primary-color);
            color: var(--primary-contrast-color);
        }
    }

    .layout-menu-button { margin-right: 0.5rem; }
    .layout-topbar-actions { margin-left: auto; display: flex; gap: 1rem; }
    .layout-config-menu   { display: flex; gap: 1rem; }
}
```

### `src/assets/layout/_menu.scss`

```scss
.layout-sidebar {
    position: fixed;
    width: 20rem;
    height: calc(100vh - 8rem);
    z-index: 999;
    overflow-y: auto;
    user-select: none;
    top: 6rem;
    left: 2rem;
    transition: transform var(--layout-section-transition-duration), left var(--layout-section-transition-duration);
    background-color: var(--surface-overlay);
    border-radius: var(--content-border-radius);
    padding: 0.5rem 1.5rem;
}

.layout-menu {
    margin: 0; padding: 0; list-style-type: none;

    .layout-root-menuitem > .layout-menuitem-root-text {
        font-size: 0.857rem;
        text-transform: uppercase;
        font-weight: 700;
        color: var(--text-color);
        margin: 0.75rem 0;
    }

    ul {
        margin: 0; padding: 0; list-style-type: none;

        a {
            display: flex;
            align-items: center;
            color: var(--text-color);
            cursor: pointer;
            padding: 0.75rem 1rem;
            border-radius: var(--content-border-radius);
            transition: box-shadow var(--element-transition-duration);

            .layout-menuitem-icon { margin-right: 0.5rem; }
            .layout-submenu-toggler { font-size: 75%; margin-left: auto; transition: transform var(--element-transition-duration); }

            &.active-route { font-weight: 700; color: var(--primary-color); }
            &:hover { background-color: var(--surface-hover); }
        }

        ul li a        { margin-left: 1rem; }
        ul ul li a     { margin-left: 2rem; }
    }
}
```

### `src/assets/layout/_responsive.scss`

```scss
@media (min-width: 992px) {
    .layout-wrapper {
        &.layout-static .layout-main-container { margin-left: 22rem; }
        &.layout-static.layout-static-inactive {
            .layout-sidebar { transform: translateX(-100%); left: 0; }
            .layout-main-container { margin-left: 0; padding-left: 2rem; }
        }
        &.layout-overlay .layout-main-container { margin-left: 0; padding-left: 2rem; }
        &.layout-overlay .layout-sidebar {
            transform: translateX(-100%);
            left: 0; top: 0; height: 100vh;
            border-top-left-radius: 0; border-bottom-left-radius: 0;
            border-right: 1px solid var(--surface-border);
        }
        &.layout-overlay-active .layout-sidebar { transform: translateX(0); }
        .layout-mask { display: none; }
    }
}

@media (max-width: 991px) {
    .blocked-scroll { overflow: hidden; }

    .layout-wrapper {
        .layout-main-container { margin-left: 0; padding-left: 2rem; }
        .layout-sidebar {
            transform: translateX(-100%);
            left: 0; top: 0; height: 100vh;
            border-top-left-radius: 0; border-bottom-left-radius: 0;
        }
        .layout-mask {
            display: none;
            position: fixed; top: 0; left: 0; z-index: 998;
            width: 100%; height: 100%;
            background-color: var(--maskbg);
        }
        &.layout-mobile-active {
            .layout-sidebar { transform: translateX(0); }
            .layout-mask    { display: block; }
        }
    }
}
```

### `src/assets/layout/_footer.scss`

```scss
.layout-footer {
    padding: 1.5rem 2rem;
    display: flex;
    align-items: center;
    justify-content: center;
    color: var(--text-color-secondary);
    font-size: 0.875rem;
}
```

### `src/assets/styles.scss`

```scss
@use 'primeicons/primeicons.css';
@use '@/assets/layout/layout.scss';
```

### `src/assets/tailwind.css`

```css
@import "tailwindcss";
@plugin "tailwindcss-primeui";
```

---

## 7. Configuración de Angular

### `angular.json` — sección de estilos

```json
"styles": [
    "src/assets/styles.scss",
    "src/assets/tailwind.css"
],
"inlineStyleLanguage": "scss"
```

### `tsconfig.json` — alias de paths

```json
{
    "compilerOptions": {
        "paths": {
            "@/*": ["src/*"]
        }
    }
}
```

### `postcss.config.mjs` (requerido por Tailwind v4)

```js
export default {
    plugins: {
        '@tailwindcss/postcss': {}
    }
};
```

---

## 8. Cambiar el color primario para QQuote

En `LayoutService`, cambiar el valor de `primary` en el estado inicial:

```typescript
layoutConfig = signal<LayoutConfig>({
    preset: 'Aura',
    primary: 'blue',    // opciones: emerald, blue, indigo, violet, orange, amber, teal, cyan, pink, etc.
    surface: null,
    darkTheme: false,
    menuMode: 'static'
});
```

Los colores disponibles en PrimeNG Aura son los colores de Tailwind. El preset `Aura` define la forma (border-radius, padding, sombras). Para cambiar la forma del preset, sustituir `Aura` por `Lara` o `Nora` en `app.config.ts`.

---

## 9. Diagrama del shell

```
┌─────────────────────────────────────────────────────────────────┐
│  TOPBAR (fixed, h-16, z-997)                                    │
│  [≡]  [LOGO QQuote]    [breadcrumb]   ...   [☀/🌙] [🎨] [👤]  │
└─────────────────────────────────────────────────────────────────┘
│                              │
│  SIDEBAR (fixed, w-80,       │  CONTENT AREA
│  top-24, left-8)             │  padding-top: 6rem (clearance topbar)
│                              │  margin-left: 22rem (en static desktop)
│  PRINCIPAL                   │
│    🏠 Dashboard              │  <router-outlet>
│                              │     → aquí renderizan las páginas de QQuote
│  MÓDULO A                    │
│    📋 Sección 1              │
│    📄 Sección 2              │
│                              │
│  MÓDULO B                    │
│    ...                       │
│                              │  FOOTER
│                              │  (al fondo del content area)
└──────────────────────────────┴──────────────────────────────────┘
```

---

## 10. Checklist de migración

- [ ] Instalar dependencias (sección 1)
- [ ] Copiar `LayoutService` → adaptar `primary` color
- [ ] Copiar los 7 componentes del layout (sección 4)
- [ ] Crear estructura SCSS en `src/assets/layout/` (sección 6)
- [ ] Registrar estilos en `angular.json` (sección 7)
- [ ] Configurar alias `@/` en `tsconfig.json`
- [ ] Configurar `postcss.config.mjs`
- [ ] Configurar `app.config.ts` con `provideZonelessChangeDetection` y `providePrimeNG` (sección 2)
- [ ] Crear ruta raíz envuelta en `AppLayout` (sección 2)
- [ ] Copiar página de login (sección 5)
- [ ] Definir `model` de navegación en `AppMenu` con rutas de QQuote (sección 4)
- [ ] Reemplazar logo en `AppTopbar` y `login.component.html`
- [ ] Actualizar texto del footer en `AppFooter`
- [ ] Actualizar `labelMap` del breadcrumb en `AppTopbar`
