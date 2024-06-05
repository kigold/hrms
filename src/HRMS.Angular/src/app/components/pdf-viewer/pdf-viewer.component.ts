import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';
import {DomSanitizer} from '@angular/platform-browser';

@Component({
  selector: 'app-pdf-viewer',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './pdf-viewer.component.html',
  styleUrl: './pdf-viewer.component.css'
})
export class PdfViewerComponent {

  @Input() filePath: string = ""
  show: boolean = false;

  constructor(private sanitizer: DomSanitizer) { }

  transform() {
    var url = this.sanitizer.bypassSecurityTrustResourceUrl(this.filePath);
    this.show = true;
    return url;
  }
  
  ngOnInit(){
    this.show = false;
    this.transform();
  }
}
