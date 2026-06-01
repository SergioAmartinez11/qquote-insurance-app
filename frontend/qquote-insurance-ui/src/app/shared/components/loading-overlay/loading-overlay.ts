import { Component, Input } from '@angular/core';
import { ProgressSpinnerModule } from 'primeng/progressspinner';

@Component({
  selector: 'app-loading-overlay',
  standalone: true,
  imports: [ProgressSpinnerModule],
  template: `
    @if (visible) {
      <div class="loading-overlay">
        <p-progress-spinner strokeWidth="4" animationDuration=".8s" styleClass="overlay-spinner" />
      </div>
    }
  `,
  styles: [
    `
      .loading-overlay {
        position: absolute;
        inset: 0;
        z-index: 10;
        display: flex;
        align-items: center;
        justify-content: center;
        background: color-mix(in srgb, var(--surface-0) 80%, transparent);
        border-radius: inherit;
      }

      :host ::ng-deep .overlay-spinner {
        width: 48px;
        height: 48px;
      }
    `,
  ],
})
export class LoadingOverlayComponent {
  @Input() visible = false;
}
