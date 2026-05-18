import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'features-widget',
  standalone: true,
  imports: [CommonModule],
  template: ` <div id="features" class="py-6 px-6 lg:px-20 mt-8 mx-0 lg:mx-20">
    <div class="grid grid-cols-12 gap-4 justify-center">
      <div class="col-span-12 text-center mt-20 mb-6">
        <div class="text-surface-900 dark:text-surface-0 font-normal mb-2 text-4xl">
          Why Choose QQuote
        </div>
        <span class="text-muted-color text-2xl"
          >Everything you need to protect your life and assets</span
        >
      </div>

      <div class="col-span-12 md:col-span-12 lg:col-span-4 p-0 lg:pr-8 lg:pb-8 mt-6 lg:mt-0">
        <div
          style="height: 160px; padding: 2px; border-radius: 10px; background: linear-gradient(90deg, rgba(253, 228, 165, 0.2), rgba(187, 199, 205, 0.2)), linear-gradient(180deg, rgba(253, 228, 165, 0.2), rgba(187, 199, 205, 0.2))"
        >
          <div class="p-4 bg-surface-0 dark:bg-surface-900 h-full" style="border-radius: 8px">
            <div
              class="flex items-center justify-center bg-yellow-200 mb-4"
              style="width: 3.5rem; height: 3.5rem; border-radius: 10px"
            >
              <i class="pi pi-fw pi-users text-2xl! text-yellow-700"></i>
            </div>
            <h5 class="mb-2 text-surface-900 dark:text-surface-0">Instant Quotes</h5>
            <span class="text-surface-600 dark:text-surface-200"
              >Get personalized insurance quotes in minutes, not days.</span
            >
          </div>
        </div>
      </div>

      <div class="col-span-12 md:col-span-12 lg:col-span-4 p-0 lg:pr-8 lg:pb-8 mt-6 lg:mt-0">
        <div
          style="height: 160px; padding: 2px; border-radius: 10px; background: linear-gradient(90deg, rgba(145, 226, 237, 0.2), rgba(251, 199, 145, 0.2)), linear-gradient(180deg, rgba(253, 228, 165, 0.2), rgba(172, 180, 223, 0.2))"
        >
          <div class="p-4 bg-surface-0 dark:bg-surface-900 h-full" style="border-radius: 8px">
            <div
              class="flex items-center justify-center bg-cyan-200 mb-4"
              style="width: 3.5rem; height: 3.5rem; border-radius: 10px"
            >
              <i class="pi pi-fw pi-palette text-2xl! text-cyan-700"></i>
            </div>
            <h5 class="mb-2 text-surface-900 dark:text-surface-0">Tailored Coverage</h5>
            <span class="text-surface-600 dark:text-surface-200"
              >Policies built around your unique needs and budget.</span
            >
          </div>
        </div>
      </div>

      <div class="col-span-12 md:col-span-12 lg:col-span-4 p-0 lg:pb-8 mt-6 lg:mt-0">
        <div
          style="height: 160px; padding: 2px; border-radius: 10px; background: linear-gradient(90deg, rgba(145, 226, 237, 0.2), rgba(172, 180, 223, 0.2)), linear-gradient(180deg, rgba(172, 180, 223, 0.2), rgba(246, 158, 188, 0.2))"
        >
          <div class="p-4 bg-surface-0 dark:bg-surface-900 h-full" style="border-radius: 8px">
            <div
              class="flex items-center justify-center bg-indigo-200"
              style="width: 3.5rem; height: 3.5rem; border-radius: 10px"
            >
              <i class="pi pi-fw pi-map text-2xl! text-indigo-700"></i>
            </div>
            <div class="mt-6 mb-1 text-surface-900 dark:text-surface-0 text-xl  font-semibold ">
              Trusted Partners
            </div>
            <span class="text-surface-600 dark:text-surface-200"
              >Access top-rated insurance carriers in one platform.</span
            >
          </div>
        </div>
      </div>

      <div class="col-span-12 md:col-span-12 lg:col-span-4 p-0 lg:pr-8 lg:pb-8 mt-6 lg:mt-0">
        <div
          style="height: 160px; padding: 2px; border-radius: 10px; background: linear-gradient(90deg, rgba(187, 199, 205, 0.2), rgba(251, 199, 145, 0.2)), linear-gradient(180deg, rgba(253, 228, 165, 0.2), rgba(145, 210, 204, 0.2))"
        >
          <div class="p-4 bg-surface-0 dark:bg-surface-900 h-full" style="border-radius: 8px">
            <div
              class="flex items-center justify-center bg-slate-200 mb-4"
              style="width: 3.5rem; height: 3.5rem; border-radius: 10px"
            >
              <i class="pi pi-fw pi-id-card text-2xl! text-slate-700"></i>
            </div>
            <div class="mt-6 mb-1 text-surface-900 dark:text-surface-0 text-xl font-semibold ">
              24/7 Support
            </div>
            <span class="text-surface-600 dark:text-surface-200"
              >Our agents are here to help whenever you need.</span
            >
          </div>
        </div>
      </div>

      <div class="col-span-12 md:col-span-12 lg:col-span-4 p-0 lg:pr-8 lg:pb-8 mt-6 lg:mt-0">
        <div
          style="height: 160px; padding: 2px; border-radius: 10px; background: linear-gradient(90deg, rgba(187, 199, 205, 0.2), rgba(246, 158, 188, 0.2)), linear-gradient(180deg, rgba(145, 226, 237, 0.2), rgba(160, 210, 250, 0.2))"
        >
          <div class="p-4 bg-surface-0 dark:bg-surface-900 h-full" style="border-radius: 8px">
            <div
              class="flex items-center justify-center bg-orange-200 mb-4"
              style="width: 3.5rem; height: 3.5rem; border-radius: 10px"
            >
              <i class="pi pi-fw pi-star text-2xl! text-orange-700"></i>
            </div>
            <div class="mt-6 mb-1 text-surface-900 dark:text-surface-0 text-xl font-semibold ">
              Best Rates
            </div>
            <span class="text-surface-600 dark:text-surface-200"
              >Compare rates and find the best value for your coverage.</span
            >
          </div>
        </div>
      </div>

      <div class="col-span-12 md:col-span-12 lg:col-span-4 p-0 lg:pb-8 mt-6 lg:mt-0">
        <div
          style="height: 160px; padding: 2px; border-radius: 10px; background: linear-gradient(90deg, rgba(251, 199, 145, 0.2), rgba(246, 158, 188, 0.2)), linear-gradient(180deg, rgba(172, 180, 223, 0.2), rgba(212, 162, 221, 0.2))"
        >
          <div class="p-4 bg-surface-0 dark:bg-surface-900 h-full" style="border-radius: 8px">
            <div
              class="flex items-center justify-center bg-pink-200 mb-4"
              style="width: 3.5rem; height: 3.5rem; border-radius: 10px"
            >
              <i class="pi pi-fw pi-moon text-2xl! text-pink-700"></i>
            </div>
            <div class="mt-6 mb-1 text-surface-900 dark:text-surface-0 text-xl font-semibold " >
              Easy Management
            </div>
            <span class="text-surface-600 dark:text-surface-200"
              >Manage all your policies from a single dashboard.</span
            >
          </div>
        </div>
      </div>

      <div class="col-span-12 md:col-span-12 lg:col-span-4 p-0 lg:pr-8 mt-6 lg:mt-0">
        <div
          style="height: 160px; padding: 2px; border-radius: 10px; background: linear-gradient(90deg, rgba(145, 210, 204, 0.2), rgba(160, 210, 250, 0.2)), linear-gradient(180deg, rgba(187, 199, 205, 0.2), rgba(145, 210, 204, 0.2))"
        >
          <div class="p-4 bg-surface-0 dark:bg-surface-900 h-full" style="border-radius: 8px">
            <div
              class="flex items-center justify-center bg-teal-200 mb-4"
              style="width: 3.5rem; height: 3.5rem; border-radius: 10px"
            >
              <i class="pi pi-fw pi-shopping-cart text-2xl! text-teal-700"></i>
            </div>
            <div class="mt-6 mb-1 text-surface-900 dark:text-surface-0 text-xl font-semibold " >
              Fast Claims
            </div>
            <span class="text-surface-600 dark:text-surface-200"
              >Simple claims process with fast turnaround times.</span
            >
          </div>
        </div>
      </div>

      <div class="col-span-12 md:col-span-12 lg:col-span-4 p-0 lg:pr-8 mt-6 lg:mt-0">
        <div
          style="height: 160px; padding: 2px; border-radius: 10px; background: linear-gradient(90deg, rgba(145, 210, 204, 0.2), rgba(212, 162, 221, 0.2)), linear-gradient(180deg, rgba(251, 199, 145, 0.2), rgba(160, 210, 250, 0.2))"
        >
          <div class="p-4 bg-surface-0 dark:bg-surface-900 h-full" style="border-radius: 8px">
            <div
              class="flex items-center justify-center bg-blue-200 mb-4"
              style="width: 3.5rem; height: 3.5rem; border-radius: 10px"
            >
              <i class="pi pi-fw pi-globe text-2xl! text-blue-700"></i>
            </div>
            <div class="mt-6 mb-1 text-surface-900 dark:text-surface-0 text-xl font-semibold " >
              Digital First
            </div>
            <span class="text-surface-600 dark:text-surface-200"
              >Fully digital experience from quote to policy issuance.</span
            >
          </div>
        </div>
      </div>

      <div class="col-span-12 md:col-span-12 lg:col-span-4 p-0 lg-4 mt-6 lg:mt-0">
        <div
          style="height: 160px; padding: 2px; border-radius: 10px; background: linear-gradient(90deg, rgba(160, 210, 250, 0.2), rgba(212, 162, 221, 0.2)), linear-gradient(180deg, rgba(246, 158, 188, 0.2), rgba(212, 162, 221, 0.2))"
        >
          <div class="p-4 bg-surface-0 dark:bg-surface-900 h-full" style="border-radius: 8px">
            <div
              class="flex items-center justify-center bg-purple-200 mb-4"
              style="width: 3.5rem; height: 3.5rem; border-radius: 10px"
            >
              <i class="pi pi-fw pi-eye text-2xl! text-purple-700"></i>
            </div>
            <div class="mt-6 mb-1 text-surface-900 dark:text-surface-0 text-xl font-semibold " >
              Data Security
            </div>
            <span class="text-surface-600 dark:text-surface-200"
              >Your personal information is protected with bank-level security.</span
            >
          </div>
        </div>
      </div>

      <div
        class="col-span-12 mt-20 mb-20 p-2 md:p-20"
        style="border-radius: 20px; background: linear-gradient(0deg, rgba(255, 255, 255, 0.6), rgba(255, 255, 255, 0.6)), radial-gradient(77.36% 256.97% at 77.36% 57.52%, #efe1af 0%, #c3dcfa 100%)"
      >
        <div class="flex flex-col justify-center items-center text-center px-4 py-4 md:py-0">
          <div class="”text-gray-900" mb-2 text-3xl font-semibold”>Sarah Thompson</div>
          <span class="”text-gray-600" text-2xl”>Small Business Owner</span>
          <p
            class="”text-gray-900"
            sm:line-height-2
            md:line-height-4
            text-2xl
            mt-6”
            style="”max-width:"
            800px”
          >
            “QQuote saved me hours of research. I compared five policies in minutes and found the
            perfect coverage for my business at a price that fit my budget. The whole process was
            seamless from quote to activation.”
          </p>
          <img
            src="https://primefaces.org/cdn/templates/sakai/landing/peak-logo.svg"
            class="mt-6"
            alt="Company logo"
          />
        </div>
      </div>
    </div>
  </div>`,
})
export class FeaturesWidget {}
