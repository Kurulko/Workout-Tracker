import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { TokenManager } from 'src/app/shared/helpers/managers/token-manager';

@Component({
    selector: 'app-logout',
    template:'<ng-content />'
})
export class LogoutComponent implements OnInit {
    constructor(private router: Router, private tokenManager: TokenManager) {}

    ngOnInit(): void {
        this.tokenManager.logout();
        this.router.navigate(['/login']);
    }
}