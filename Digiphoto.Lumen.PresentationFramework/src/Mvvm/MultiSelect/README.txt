
Problema:

La ListBox permette di selezionare più di un elemento tramite il SelectionMode="Extended".

Purtoppo però la proprietà SelectedItems non è bindabile ed è readonly (in pratica WPF 4 attualmente
non da supporto per questa cosa.

Per ovviare, si potevano trovare degli stratagemmi, oppure implementare la soluzione che fosse nell'ottica
di wpf (cosi magari nelle prossime versioni di WPF verrà implementata nativamente da mamma Microsoft).

Per una discussione completa vedere qui:

http://grokys.blogspot.com/2012/02/mvvm-and-multiple-selection-part-iv.html
E' una storia a quattro puntate.

Una soluzione più semplice da capire poteva essere anche questa, ma non mi soddisfava al massimo:
http://www.gbogea.com/2010/01/02/mvvm-multiselect-listbox



