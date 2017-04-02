u s i n g   S y s t e m ;  
 u s i n g   S y s t e m . C o l l e c t i o n s . G e n e r i c ;  
 u s i n g   S y s t e m . L i n q ;  
 u s i n g   S y s t e m . T e x t ;  
 u s i n g   U n i t y E n g i n e ;  
 n a m e s p a c e   A i r Q u a l i t y  
 { 	 	 	 	 	 	 	 	 	 	 	 / *   a   c l a s s   d e s c r i b i n g   t o t a l   a t m o s p h e r i c   c o m p o s i t i o n   o f   a   h a b i t a b l e   v o l u m e   * /  
 	 p u b l i c   c l a s s   A Q A i r   :   D i c t i o n a r y < s t r i n g ,   A Q G a s > ,   I C o n f i g N o d e  
 	 {  
 	 	 p u b l i c   f l o a t   T e m p e r a t u r e ;  
 	 	 p u b l i c   b o o l   C o n t a i n s G a s ( s t r i n g   g a s n a m e )  
 	 	 {  
 	 	 }  
 	 	 p u b l i c   v o i d   R e g i s t e r G a s ( s t r i n g   g a s n a m e )  
 	 	 {  
 	 	 	 i f   ( C o n t a i n s K e y ( g a s n a m e ) )  
 	 	 	 {  
 	 	 	 	 r e t u r n ;  
 	 	 	 }  
 	 	 	 A i r . A d d ( g a s n a m e ,   n e w   A Q G a s ( ) ) ;  
 	 	 	 A i r [ g a s n a m e ] . L o n g N a m e   =   g a s n a m e ;  
 	 	 	 A i r [ g a s n a m e ] . P r e s s u r e   =   0 . 0 f ;  
 	 	 	 f o r e a c h   ( C o n f i g N o d e   A Q G a s L i b r a r y N o d e   i n   G a m e D a t a b a s e . I n s t a n c e . G e t C o n f i g N o d e s ( A Q N o d e N a m e s . G a s L i b r a r y ) )  
 	 	 	 {  
 	 	 	 	 i f   ( A Q G a s L i b r a r y N o d e . H a s N o d e ( g a s n a m e ) )  
 	 	 	 	 {  
 	 	 	 	 	 A i r [ g a s n a m e ] . L o a d I n v a r i a n t ( A Q G a s L i b r a r y N o d e . G e t N o d e ( g a s n a m e ) ) ;  
 	 	 	 	 }  
 	 	 	 }  
 	 	 }  
 	 	 p u b l i c   b o o l   I s B r e a t h e a b l e ( )  
 	 	 {  
 	 	 	 f o r e a c h   ( s t r i n g   G a s E n t r y   i n   K e y s )  
 	 	 	 {  
 	 	 	 	 i f   ( t h i s [ G a s E n t r y ] . i s P o i s o n ( )   & &   ( t h i s [ G a s E n t r y ] . P r e s s u r e   >   t h i s [ G a s E n t r y ] . M a x T o l e r a t e d P r e s s u r e ) )  
 	 	 	 	 {  
 	 	 	 	 	 r e t u r n   f a l s e ; 	 / / p o i s o n o u s  
 	 	 	 	 }  
 	 	 	 }  
 	 	 	 f o r e a c h   ( s t r i n g   G a s E n t r y   i n   K e y s )  
 	 	 	 {  
 	 	 	 	 i f   ( t h i s [ G a s E n t r y ] . i s B r e a t h e a b l e ( )   & &   ( t h i s [ G a s E n t r y ] . P r e s s u r e   >   t h i s [ G a s E n t r y ] . M i n R e q u i r e d P r e s s u r e ) )  
 	 	 	 	 {  
 	 	 	 	 	 r e t u r n   t r u e ; 	 / / b r e a t h e a b l e   a n d   n o t   p o i s o n o u s  
 	 	 	 	 }  
 	 	 	 }  
 	 	 	 r e t u r n   f a l s e ; 	 	 	 / / u n b r e a t h e a b l e  
 	 	 }  
 	 	 p u b l i c   b o o l   I s P r e s s u r i s e d ( )  
 	 	 {  
 	 	 	 d o u b l e   T o t a l P r e s s u r e   =   0 . 0 f ;  
 	 	 	 f o r e a c h   ( s t r i n g   G a s E n t r y   i n   K e y s )  
 	 	 	 {  
 	 	 	 	 T o t a l P r e s s u r e   + =   t h i s [ G a s E n t r y ] . P r e s s u r e ;  
 	 	 	 }  
 	 	 	 r e t u r n   ( T o t a l P r e s s u r e   >   A Q P h y s i c a l C o n s t a n t s . A r m s t r o n g L i m i t ) ;  
 	 	 }  
 	 	 p u b l i c   f l o a t   T o t a l N a r c o t i c P o t e n t i a l ( )  
 	 	 {  
 	 	 	 d o u b l e   l _ T o t a l N a r c o t i c P o t e n t i a l   =   0 . 0 f ;  
 	 	 	 f o r e a c h   ( s t r i n g   G a s E n t r y   i n   K e y s )  
 	 	 	 {  
 	 	 	 	 l _ T o t a l N a r c o t i c P o t e n t i a l   + =   t h i s [ G a s E n t r y ] . N a r c o t i c P o t e n t i a l   *   t h i s [ G a s E n t r y ] . P r e s s u r e ;    
 	 	 	 }  
 	 	 	 r e t u r n   ( f l o a t ) l _ T o t a l N a r c o t i c P o t e n t i a l ;  
 	 	 }  
 	 	 p u b l i c   v o i d   L o a d ( C o n f i g N o d e   A Q A i r N o d e )  
 	 	 {  
 	 	 	 f o r e a c h   ( C o n f i g N o d e   G a s N o d e   i n   A Q A i r N o d e . G e t N o d e s ( ) )  
 	 	 	 {  
 	 	 	 	 i f   ( G a s N o d e . H a s V a l u e ( " L o n g N a m e " ) )  
 	 	 	 	 {  
 	 	 	 	 	 A d d ( G a s N o d e . G e t V a l u e ( " L o n g N a m e " ) ,   n e w   A Q G a s ( ) ) ;  
 	 	 	 	 	 t h i s [ G a s N o d e . G e t V a l u e ( " L o n g N a m e " ) ] . L o a d ( G a s N o d e ) ;  
 	 	 	 	 	 f o r e a c h   ( C o n f i g N o d e   A Q G a s L i b r a r y N o d e   i n   G a m e D a t a b a s e . I n s t a n c e . G e t C o n f i g N o d e s ( A Q N o d e N a m e s . G a s L i b r a r y ) )  
 	 	 	 	 	 {  
 	 	 	 	 	 	 i f   ( A Q G a s L i b r a r y N o d e . H a s N o d e ( G a s N o d e . G e t V a l u e ( " L o n g N a m e " ) ) )  
 	 	 	 	 	 	 {  
 	 	 	 	 	 	 	 t h i s [ G a s N o d e . G e t V a l u e ( " L o n g N a m e " ) ] . L o a d I n v a r i a n t ( A Q G a s L i b r a r y N o d e . G e t N o d e ( G a s N o d e . G e t V a l u e ( " L o n g N a m e " ) ) ) ;  
 	 	 	 	 	 	 }  
 	 	 	 	 	 }  
 	 	 	 	 }  
 	 	 	 }  
 	 	 	 r e t u r n ;  
 	 	 }  
 	 	 p u b l i c   v o i d   S a v e ( C o n f i g N o d e   A Q A i r N o d e )  
 	 	 {  
 	 	 	 C o n f i g N o d e   G a s N o d e ; 	  
 	 	 	 f o r e a c h   ( s t r i n g   G a s E n t r y   i n   K e y s )  
 	 	 	 {  
 	 	 	 	 i f   ( A Q A i r N o d e . H a s N o d e ( t h i s [ G a s E n t r y ] . L o n g N a m e ) )  
 	 	 	 	 {  
 	 	 	 	 	 G a s N o d e   =   A Q A i r N o d e . G e t N o d e ( t h i s [ G a s E n t r y ] . L o n g N a m e ) ;  
 	 	 	 	 }  
 	 	 	 	 e l s e  
 	 	 	 	 {  
 	 	 	 	 	 G a s N o d e   =   A Q A i r N o d e . A d d N o d e ( t h i s [ G a s E n t r y ] . L o n g N a m e ) ;  
 	 	 	 	 }  
 	 	 	 	 t h i s [ G a s E n t r y ] . S a v e ( G a s N o d e ) ;  
 	 	 	 }  
 	 	 	 r e t u r n ;  
 	 	 }  
 	 }  
 }  
 