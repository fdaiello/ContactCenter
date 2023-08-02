import { Routes, RouterModule } from '@angular/router';

import { HomeComponent } from './home';

const routes: Routes = [
    { path: '', component: HomeComponent },
    {
        path: "crm",
        loadChildren: () =>
          import("./crm/crm.module").then(m => m.CrmModule),
    },

    // otherwise redirect to home
    { path: '**', redirectTo: '' }
];

export const appRoutingModule = RouterModule.forRoot(routes, { relativeLinkResolution: 'legacy' });
