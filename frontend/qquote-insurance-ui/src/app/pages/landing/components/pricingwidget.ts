import { Component } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { DividerModule } from 'primeng/divider';
import { RippleModule } from 'primeng/ripple';

@Component({
  selector: 'pricing-widget',
  imports: [DividerModule, ButtonModule, RippleModule],
  template: `
    <div id="pricing" class="py-6 px-6 lg:px-20 my-2 md:my-6">
      <div class="text-center mb-6">
        <div class="text-surface-900 dark:text-surface-0 font-normal mb-2 text-4xl">
          Simple, Transparent Plans
        </div>
        <span class="text-muted-color text-2xl"
          >Choose the coverage plan that fits your lifestyle and budget</span
        >
      </div>

      <div class="grid grid-cols-12 gap-4 justify-between mt-20 md:mt-0">
        <div class="col-span-12 lg:col-span-4 p-0 md:p-4">
          <div
            class="p-4 flex flex-col border-surface-200 dark:border-surface-600 pricing-card cursor-pointer border-2 hover:border-primary duration-300 transition-all"
            style="border-radius: 10px"
          >
            <div class="text-surface-900 dark:text-surface-0 text-center my-8 text-3xl">Basic</div>
            <img
              src="https://primefaces.org/cdn/templates/sakai/landing/free.svg"
              class="w-10/12 mx-auto"
              alt="basic"
            />
            <div class="my-8 flex flex-col items-center gap-4">
              <div class="flex items-center">
                <span class="text-5xl font-bold mr-2 text-surface-900 dark:text-surface-0"
                  >$15</span
                >
                <span class="text-surface-600 dark:text-surface-200">/ month</span>
              </div>
              <button
                pButton
                pRipple
                label="Get Started"
                class="p-button-rounded border-0 ml-4 font-light leading-tight bg-blue-500 text-white"
                routerLink="/auth/login"
              ></button>
            </div>
            <p-divider class="w-full bg-surface-200"></p-divider>
            <ul class="my-8 list-none p-0 flex text-surface-900 dark:text-surface-0 flex-col px-8">
              <li class="py-2">
                <i class="pi pi-fw pi-check text-xl text-cyan-500 mr-2"></i>
                <span class="text-xl leading-normal">Basic Liability Coverage</span>
              </li>
              <li class="py-2">
                <i class="pi pi-fw pi-check text-xl text-cyan-500 mr-2"></i>
                <span class="text-xl leading-normal">Personal Accident Protection</span>
              </li>
              <li class="py-2">
                <i class="pi pi-fw pi-check text-xl text-cyan-500 mr-2"></i>
                <span class="text-xl leading-normal">Email Support</span>
              </li>
              <li class="py-2">
                <i class="pi pi-fw pi-check text-xl text-cyan-500 mr-2"></i>
                <span class="text-xl leading-normal">Digital Policy Documents</span>
              </li>
            </ul>
          </div>
        </div>

        <div class="col-span-12 lg:col-span-4 p-0 md:p-4 mt-6 md:mt-0">
          <div
            class="p-4 flex flex-col border-surface-200 dark:border-surface-600 pricing-card cursor-pointer border-2 hover:border-primary duration-300 transition-all"
            style="border-radius: 10px"
          >
            <div class="text-surface-900 dark:text-surface-0 text-center my-8 text-3xl">
              Standard
            </div>
            <img
              src="https://primefaces.org/cdn/templates/sakai/landing/startup.svg"
              class="w-10/12 mx-auto"
              alt="standard"
            />
            <div class="my-8 flex flex-col items-center gap-4">
              <div class="flex items-center">
                <span class="text-5xl font-bold mr-2 text-surface-900 dark:text-surface-0"
                  >$45</span
                >
                <span class="text-surface-600 dark:text-surface-200">/ month</span>
              </div>
              <button
                pButton
                pRipple
                label="Get Started"
                class="p-button-rounded border-0 ml-4 font-light leading-tight bg-blue-500 text-white"
                routerLink="/auth/login"
              ></button>
            </div>
            <p-divider class="w-full bg-surface-200"></p-divider>
            <ul class="my-8 list-none p-0 flex text-surface-900 dark:text-surface-0 flex-col px-8">
              <li class="py-2">
                <i class="pi pi-fw pi-check text-xl text-cyan-500 mr-2"></i>
                <span class="text-xl leading-normal">All Basic Features</span>
              </li>
              <li class="py-2">
                <i class="pi pi-fw pi-check text-xl text-cyan-500 mr-2"></i>
                <span class="text-xl leading-normal">Multi-Vehicle Coverage</span>
              </li>
              <li class="py-2">
                <i class="pi pi-fw pi-check text-xl text-cyan-500 mr-2"></i>
                <span class="text-xl leading-normal">24/7 Phone Support</span>
              </li>
              <li class="py-2">
                <i class="pi pi-fw pi-check text-xl text-cyan-500 mr-2"></i>
                <span class="text-xl leading-normal">Priority Claims Processing</span>
              </li>
            </ul>
          </div>
        </div>

        <div class="col-span-12 lg:col-span-4 p-0 md:p-4 mt-6 md:mt-0">
          <div
            class="p-4 flex flex-col border-surface-200 dark:border-surface-600 pricing-card cursor-pointer border-2 hover:border-primary duration-300 transition-all"
            style="border-radius: 10px"
          >
            <div class="text-surface-900 dark:text-surface-0 text-center my-8 text-3xl">
              Premium
            </div>
            <img
              src="https://primefaces.org/cdn/templates/sakai/landing/enterprise.svg"
              class="w-10/12 mx-auto"
              alt="premium"
            />
            <div class="my-8 flex flex-col items-center gap-4">
              <div class="flex items-center">
                <span class="text-5xl font-bold mr-2 text-surface-900 dark:text-surface-0"
                  >$89</span
                >
                <span class="text-surface-600 dark:text-surface-200">/ month</span>
              </div>
              <button
                pButton
                pRipple
                label="Get Started"
                class="p-button-rounded border-0 ml-4 font-light leading-tight bg-blue-500 text-white"
                routerLink="/auth/login"
              ></button>
            </div>
            <p-divider class="w-full bg-surface-200"></p-divider>
            <ul class="my-8 list-none p-0 flex text-surface-900 dark:text-surface-0 flex-col px-8">
              <li class="py-2">
                <i class="pi pi-fw pi-check text-xl text-cyan-500 mr-2"></i>
                <span class="text-xl leading-normal">All Standard Features</span>
              </li>
              <li class="py-2">
                <i class="pi pi-fw pi-check text-xl text-cyan-500 mr-2"></i>
                <span class="text-xl leading-normal">Comprehensive Coverage</span>
              </li>
              <li class="py-2">
                <i class="pi pi-fw pi-check text-xl text-cyan-500 mr-2"></i>
                <span class="text-xl leading-normal">Dedicated Personal Agent</span>
              </li>
              <li class="py-2">
                <i class="pi pi-fw pi-check text-xl text-cyan-500 mr-2"></i>
                <span class="text-xl leading-normal">Same-Day Claims Resolution</span>
              </li>
            </ul>
          </div>
        </div>
      </div>
    </div>
  `,
})
export class PricingWidget {}
